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

    private LoadedPropertySearch(string property)
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
    /// Removes casts and delegate wrappers around an operation.
    /// </summary>
    /// <param name="operation">Operation.</param>
    /// <returns>Operation without wrappers.</returns>
    public static IOperation? SkipConversions(IOperation? operation)
        => operation switch
        {
            IConversionOperation conversion => SkipConversions(conversion.Operand),
            IDelegateCreationOperation delegateCreation => SkipConversions(delegateCreation.Target),
            _ => operation,
        };

    /// <summary>
    /// Checks a value. All parts of the value belong to the same statement, so the position stays the same.
    /// </summary>
    private bool IsLoadedInValue(IOperation? value, CodePosition position)
    {
        value = SkipConversions(value);
        if (value is IAwaitOperation awaitOperation)
        {
            value = SkipConversions(awaitOperation.Operation);
        }

        return value switch
        {
            _ when VariableAssignment.Of(value) is { } variable => IsLoadedInVariable(variable, position),
            IPropertyReferenceOperation propertyReference when ReturnsElements(propertyReference.Property)
                => IsLoadedInValue(propertyReference.Instance, position),
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
    /// Looks backward from the position for the last assignment of the variable on every path.
    /// </summary>
    private bool IsLoadedInVariable(object variable, CodePosition position)
    {
        foreach (var previous in position.StatementsBefore())
        {
            var assignment = VariableAssignment.Find(previous.Statement!, variable);
            if (assignment is null)
            {
                continue;
            }

            // "user = value".
            if (assignment.PropertyName is null)
            {
                return IsLoadedInValue(assignment.Value, previous);
            }

            // "user.Profile = value". Other properties do not matter, keep looking.
            if (assignment.PropertyName == property)
            {
                return true;
            }
        }

        if (position.Block.Kind == BasicBlockKind.Entry)
        {
            return IsLoadedAtGraphStart(variable, position.FlowGraph);
        }

        if (!searchedBlocks.Add((position.Block, variable)))
        {
            return true;
        }

        return position.Block.Predecessors.All(incomingJump =>
            IsLoadedInVariable(variable, CodePosition.EndOfBlock(position.FlowGraph, incomingJump.Source)));
    }

    /// <summary>
    /// The search reached the start of a method, local function or lambda without finding an assignment.
    /// </summary>
    private bool IsLoadedAtGraphStart(object variable, FlowGraph flowGraph)
    {
        if (flowGraph.Lambda is null || flowGraph.LambdaCreatedAt is null)
        {
            // A method parameter is loaded if the method requires it with [IncludeRequired].
            return variable is IParameterSymbol { ContainingSymbol: IMethodSymbol method } parameter &&
                   AttributeHelper.MethodHasIncludeRequiredAttribute(method, parameter.Name, property);
        }

        if (variable is IParameterSymbol lambdaParameter &&
            SymbolEqualityComparer.Default.Equals(lambdaParameter.ContainingSymbol, flowGraph.Lambda.Symbol))
        {
            // users.Select(u => ...): u is an element of users. Parameters of other lambdas are unknown.
            var elementSource = LinqMethods.GetElementSource(flowGraph.Lambda);
            return lambdaParameter.Ordinal == 0 &&
                   elementSource is not null &&
                   IsLoadedInValue(elementSource, flowGraph.LambdaCreatedAt);
        }

        // A variable captured by the lambda: continue before the statement that creates the lambda.
        return IsLoadedInVariable(variable, flowGraph.LambdaCreatedAt);
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
