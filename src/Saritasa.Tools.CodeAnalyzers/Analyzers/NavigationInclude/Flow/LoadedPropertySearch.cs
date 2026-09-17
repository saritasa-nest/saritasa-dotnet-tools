using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Answers one question: "is the navigation property loaded in this value?".
/// </summary>
/// <remarks>
/// For a variable the search looks backward from the place where it is used for the last assignment,
/// the same way a person reads code. When several paths lead to the place (if/else), the property
/// must be loaded on every path.
/// </remarks>
internal sealed class LoadedPropertySearch
{
    private readonly string property;

    /// <summary>
    /// Blocks already searched for a variable. Around a loop the search comes back to the same block;
    /// that path adds nothing new, and without this set the search would never stop.
    /// </summary>
    private readonly HashSet<(BasicBlock Block, object Variable)> searchedBlocks = new();

    public LoadedPropertySearch(string property)
    {
        this.property = property;
    }

    /// <summary>
    /// Returns true if the navigation property is loaded in the value.
    /// </summary>
    /// <param name="value">Value, e.g. a call argument or a returned expression.</param>
    /// <param name="property">Navigation property name.</param>
    /// <param name="position">Position of the statement that contains the value.</param>
    /// <returns>True if the property is loaded on every path.</returns>
    public static bool IsLoaded(IOperation value, string property, CodePosition position)
        => new LoadedPropertySearch(property).IsLoadedInValue(value, position);

    /// <summary>
    /// Checks a value. All parts of the value belong to the same statement, so the position stays the same.
    /// </summary>
    private bool IsLoadedInValue(IOperation value, CodePosition position)
    {
        // Values come with implicit conversions: "IEnumerable<User> users = query.ToList()",
        // "list.Where(...)" (the list is converted to IEnumerable<User>). Without skipping them a call or
        // a property below is not recognized.
        value = RoslynHelper.SkipWrappers(value);

        return value switch
        {
            // local refs, parameters, lowered operators, out vars
            _ when VariableAssignment.Of(value) is { } variable => IsLoadedInVariable(variable, position),

            // await GetUserAsync(): the entities come from the awaited value.
            IAwaitOperation awaitOperation => IsLoadedInValue(awaitOperation.Operation, position),

            // Indexers, Enumerator properties
            IPropertyReferenceOperation propertyReference when ReturnsElements(propertyReference.Property)
                => IsLoadedInValue(propertyReference.Instance, position),

            // Method result
            IInvocationOperation call => IsLoadedInCallResult(call, position),

            // dbContext.Users, new User(), null, fields: not loaded.
            _ => false,
        };
    }

    private bool IsLoadedInCallResult(IInvocationOperation call, CodePosition position)
    {
        if (AttributeHelper.MethodHasIncludesAttribute(call.TargetMethod, property) ||
            LinqMethods.GetIncludedProperty(call) == property)
        {
            return true;
        }

        var entitiesSource = GetEntitiesSource(call);
        return entitiesSource is not null && IsLoadedInValue(entitiesSource, position);
    }

    /// <summary>
    /// Looks backward from the statement for the last assignment of the variable on every path.
    /// The statement itself is not checked.
    /// </summary>
    private bool IsLoadedInVariable(object variable, CodePosition position)
        => IsLoadedInVariable(variable, position.FlowGraph, position.Block, position.StatementsBefore());

    /// <summary>
    /// Looks for the last assignment of the variable in the given statements of the block, then in the blocks
    /// that jump to it.
    /// </summary>
    /// <param name="variable">Variable.</param>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <param name="statementsToCheck">Statements of the block to check, the nearest first.</param>
    private bool IsLoadedInVariable(
        object variable,
        IFlowGraph flowGraph,
        BasicBlock block,
        IEnumerable<CodePosition> statementsToCheck)
    {
        foreach (var previous in statementsToCheck)
        {
            var assignment = VariableAssignment.Find(previous.Statement, variable);
            if (assignment is null)
            {
                continue;
            }

            // "user = value".
            if (assignment.PropertyName is null)
            {
                return IsLoadedInValue(assignment.Value, previous);
            }

            // "user.Profile = value". Someone set property which requires include manually.
            if (assignment.PropertyName == property)
            {
                return true;
            }
        }

        if (block.Kind == BasicBlockKind.Entry)
        {
            return IsLoadedAtGraphStart(variable, flowGraph);
        }

        if (!searchedBlocks.Add((block, variable)))
        {
            return true;
        }

        // The first block of a catch, filter or finally is entered from the try block, and may also have jumps
        // from inside the handler (a loop at its start).
        var tryRegion = FindTryRegion(block);
        if (tryRegion is null && block.Predecessors.IsEmpty)
        {
            // No known way to get here: not loaded.
            return false;
        }

        // A jump leaves the source block after all its statements, so all of them are checked.
        return block.Predecessors.All(incomingJump => IsLoadedInVariable(
                   variable,
                   flowGraph,
                   incomingJump.Source,
                   CodePosition.AllInBlock(flowGraph, incomingJump.Source).Reverse())) &&
               (tryRegion is null || IsLoadedInTryRegion(variable, tryRegion, flowGraph));
    }

    /// <summary>
    /// The search reached the start of a catch, filter or finally block. The graph has no jumps for exceptions:
    /// an exception can leave the try block after any statement, so the property must be loaded before
    /// the try block and after every statement in it.
    /// </summary>
    private bool IsLoadedInTryRegion(object variable, ControlFlowRegion tryRegion, IFlowGraph flowGraph)
        => Enumerable
            .Range(tryRegion.FirstBlockOrdinal, tryRegion.LastBlockOrdinal - tryRegion.FirstBlockOrdinal + 1)
            .Select(ordinal => flowGraph.Graph.Blocks[ordinal])
            .All(block =>
            {
                var statements = CodePosition.AllInBlock(flowGraph, block).ToList();

                // Count 0: the exception is thrown before the block's first statement; count N: after statement N.
                return Enumerable
                    .Range(0, statements.Count + 1)
                    .All(count => IsLoadedInVariable(variable, flowGraph, block, statements.Take(count).Reverse()));
            });

    /// <summary>
    /// For the first block of a catch, filter or finally: the try region it handles. Null for other blocks.
    /// </summary>
    /// <remarks>
    /// "try/catch" is a TryAndCatch region with nested Try and Catch regions; "catch when" wraps the handler
    /// into FilterAndHandler; "try/finally" is TryAndFinally. The try region is always the first nested region.
    /// </remarks>
    private static ControlFlowRegion? FindTryRegion(BasicBlock block)
    {
        var region = block.EnclosingRegion;
        while (region.EnclosingRegion is { } parent && region.FirstBlockOrdinal == block.Ordinal)
        {
            if (parent.Kind is ControlFlowRegionKind.TryAndCatch or ControlFlowRegionKind.TryAndFinally &&
                parent.NestedRegions[0] != region)
            {
                return parent.NestedRegions[0];
            }

            region = parent;
        }

        return null;
    }

    /// <summary>
    /// The search reached the start of a method, local function or lambda without finding an assignment.
    /// </summary>
    private bool IsLoadedAtGraphStart(object variable, IFlowGraph flowGraph)
    {
        if (flowGraph is not LambdaFlowGraph lambdaGraph)
        {
            // A method parameter is loaded if the method requires it with [IncludeRequired].
            return variable is IParameterSymbol { ContainingSymbol: IMethodSymbol method } parameter &&
                   AttributeHelper.MethodHasIncludeRequiredAttribute(method, parameter.Name, property);
        }

        if (variable is IParameterSymbol lambdaParameter &&
            SymbolEqualityComparer.Default.Equals(lambdaParameter.ContainingSymbol, lambdaGraph.Lambda.Symbol))
        {
            // users.Select(u => ...): u is an element of users. Parameters of other lambdas are unknown.
            var elementSource = LinqMethods.GetContainer(lambdaGraph.Lambda);
            return lambdaParameter.Ordinal == 0 &&
                   elementSource is not null &&
                   IsLoadedInValue(elementSource, lambdaGraph.CreatedAt);
        }

        // A variable captured by the lambda: continue before the statement that creates the lambda.
        return IsLoadedInVariable(variable, lambdaGraph.CreatedAt);
    }

    /// <summary>
    /// Maps a call to the value whose entities it returns: <c>query</c> for <c>query.Where(...)</c>. Null if the
    /// call returns other objects (<c>Select</c>, arbitrary methods).
    /// </summary>
    private static IOperation? GetEntitiesSource(IInvocationOperation call)
        => call.TargetMethod.Name switch
        {
            // foreach is rewritten by the compiler to "enumerator = collection.GetEnumerator()".

            "GetEnumerator" or "GetAsyncEnumerator" => call.Instance,
            // dictionary.GetValueOrDefault(id): an extension method, the dictionary is the first argument.
            "GetValueOrDefault" => LinqMethods.GetSource(call),

            _ when LinqMethods.KeepsSourceEntities(call.TargetMethod) => LinqMethods.GetSource(call),
            _ => null,
        };

    /// <summary>
    /// Properties returning elements of the collection they are called on:
    /// "item = enumerator.Current" (foreach is rewritten by the compiler to it), users[0], dictionary[id],
    /// dictionary.Values, pair.Value of a dictionary entry.
    /// </summary>
    private static bool ReturnsElements(IPropertySymbol property)
        => property.IsIndexer ||
           property.Name is "Current" or "Values" ||
           (property.Name == "Value" && property.ContainingType.Name == "KeyValuePair");
}
