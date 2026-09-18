using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Where a variable's value comes from, read backward from the place the variable is used.
/// </summary>
/// <remarks>
/// The counterpart of <see cref="ExpressionOrigin"/>, and the half that has to walk: a variable gets its value
/// from the last assignment before the place it is read, which may be in another block, in another body, or on
/// several paths at once. It is also the only half with memory, see <see cref="searchedBlocks"/>.
/// A variable is a local, a parameter or a compiler temporary (<see cref="CaptureId"/>),
/// see <see cref="GetRelatedVariable"/>.
/// </remarks>
internal sealed class VariableOrigin
{
    private readonly string property;

    /// <summary>
    /// Blocks already read for a variable. Around a loop the walk comes back to the same block; that path adds
    /// nothing new, and without this set the walk would never stop.
    /// </summary>
    private readonly HashSet<(BasicBlock Block, object Variable)> searchedBlocks = new();

    /// <summary>
    /// Initializes the walk.
    /// </summary>
    /// <param name="property">Navigation property the search is looking for.</param>
    public VariableOrigin(string property)
    {
        this.property = property;
    }

    /// <summary>
    /// Returns the local, parameter or compiler temporary the operation reads; null for anything else.
    /// </summary>
    /// <param name="operation">Operation.</param>
    /// <returns>Variable or null.</returns>
    public static object? GetRelatedVariable(IOperation? operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation switch
        {
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            IFlowCaptureReferenceOperation capture => capture.Id,
            IDeclarationExpressionOperation declaration => GetRelatedVariable(declaration.Expression), // "out var user"
            _ => null,
        };
    }

    /// <summary>
    /// Returns where the variable's value comes from, by reading the statements above the position upward.
    /// More than one origin when several paths lead to the position, and then every one of them has to have
    /// the property loaded.
    /// </summary>
    /// <param name="variable">Variable, see <see cref="GetRelatedVariable"/>.</param>
    /// <param name="position">Place the variable is read at.</param>
    /// <returns>Origins; never empty.</returns>
    public IEnumerable<Origin> GetOrigins(object variable, CodePosition position)
    {
        foreach (var previous in position.GetPreviousStatements())
        {
            if (GetOriginInStatement(previous, variable) is { } assigned)
            {
                return [assigned];
            }
        }

        // No assignment in this block, so the value was already there when the block started.
        return GetOriginsBeforeBlock(variable, position.FlowGraph, position.Block);
    }

    /// <summary>
    /// Returns the origin the statement gives the variable, or null when the statement leaves the variable
    /// alone and the walk has to keep reading upward.
    /// </summary>
    private Origin? GetOriginInStatement(CodePosition statement, object variable)
    {
        if (statement.Statement is not { } statementOperation)
        {
            return null;
        }

        var operation = statementOperation is IExpressionStatementOperation expressionStatement
            ? expressionStatement.Operation
            : statementOperation;

        return operation switch
        {
            // "user = ..." and "var user = ...": the variable comes from the assigned value.
            ISimpleAssignmentOperation assignment when AreSame(GetRelatedVariable(assignment.Target), variable)
                => Origin.Create(assignment.Value, statement),

            // "user.Profile = ..." (the compiler also rewrites "new User { Profile = ... }" into this form).
            // Another property says nothing, so the walk keeps reading upward.
            ISimpleAssignmentOperation { Target: IPropertyReferenceOperation reference }
                when AreSame(GetRelatedVariable(reference.Instance), variable)
                => reference.Property.Name == property ? Origin.Loaded : null,

            // "#1 = ...": a temporary value the compiler creates for ?:, ??, object initializers and foreach.
            IFlowCaptureOperation capture when AreSame(capture.Id, variable)
                => Origin.Create(capture.Value, statement),

            // "var (id, user) = pair" and "foreach (var (id, user) in dictionary)": user is a part of the pair.
            IDeconstructionAssignmentOperation deconstruction
                when deconstruction.Target.DescendantsAndSelf()
                    .Any(target => AreSame(GetRelatedVariable(target), variable))
                => Origin.Create(deconstruction.Value, statement),

            _ => GetOutArgumentOrigin(statementOperation, statement, variable),
        };
    }

    /// <summary>
    /// No statement of the block assigns the variable, so its value comes from before the block: from every way
    /// control can enter it.
    /// </summary>
    private IEnumerable<Origin> GetOriginsBeforeBlock(object variable, FlowGraph flowGraph, BasicBlock block)
    {
        if (block.Kind == BasicBlockKind.Entry)
        {
            // This is the first block, so there is nothing before it inside the body.
            return GetOriginsBeforeBody(variable, flowGraph);
        }

        if (!searchedBlocks.Add((block, variable)))
        {
            // A loop came back to a block the walk already read: this path adds nothing new.
            return [Origin.Loaded];
        }

        // Unreachable code has no way in, and nothing is known about the variable there.
        return GetWaysIntoBlock(flowGraph, block)
            .SelectMany(wayIn => GetOrigins(variable, wayIn))
            .DefaultIfEmpty(Origin.NotFound);
    }

    /// <summary>
    /// Nothing inside the body of the method, local function or lambda assigns the variable, so its value comes
    /// from outside: it is a parameter, or a variable a lambda captured.
    /// </summary>
    private IEnumerable<Origin> GetOriginsBeforeBody(object variable, FlowGraph flowGraph)
    {
        if (flowGraph.CreationStatement is not { } lambdaCreation)
        {
            // The caller of a method that asks for the property with [IncludeRequired] is the one that loads it.
            return [HasIncludeRequiredAttribute(variable) ? Origin.Loaded : Origin.NotFound];
        }

        if (IsParameterOfMethod(variable, flowGraph.Method))
        {
            // "users.Select(u => ...)": the first parameter is an element of users. The other parameters
            // ("Select((u, i) => ...)", lambdas of non-LINQ methods) are unknown.
            var elements = IsFirstParameter(variable) ? flowGraph.GetLambdaElementsSource() : null;

            return [Origin.Create(elements, lambdaCreation)];
        }

        // A variable captured by the lambda: keep reading before the statement that creates the lambda.
        return GetOrigins(variable, lambdaCreation);
    }

    /// <summary>
    /// Places control can enter the block from: the end of every block that jumps to it and, for the first block
    /// of a catch, filter or finally, every place of the try block it handles.
    /// </summary>
    /// <remarks>
    /// The graph has no jumps for exceptions, so a handler looks unreachable. An exception can leave the try block
    /// after any of its statements, which is why every place of it is a way into the handler.
    /// </remarks>
    private static IEnumerable<CodePosition> GetWaysIntoBlock(FlowGraph flowGraph, BasicBlock block)
    {
        // A jump leaves its block after all the statements, so all of them are read.
        foreach (var incomingJump in block.Predecessors)
        {
            yield return CodePosition.GetPositionAtBlockEnd(flowGraph, incomingJump.Source);
        }

        if (FindHandledTryRegion(block) is not { } tryRegion)
        {
            yield break;
        }

        for (var ordinal = tryRegion.FirstBlockOrdinal; ordinal <= tryRegion.LastBlockOrdinal; ordinal++)
        {
            foreach (var position in CodePosition.GetEveryPositionInBlock(flowGraph, flowGraph.Graph.Blocks[ordinal]))
            {
                yield return position;
            }
        }
    }

    /// <summary>
    /// For the first block of a catch, filter or finally: the try region it handles. Null for other blocks.
    /// </summary>
    /// <remarks>
    /// "try/catch" is a TryAndCatch region with nested Try and Catch regions; "catch when" wraps the handler
    /// into FilterAndHandler; "try/finally" is TryAndFinally. The try region is always the first nested region.
    /// </remarks>
    private static ControlFlowRegion? FindHandledTryRegion(BasicBlock block)
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
    /// "dictionary.TryGetValue(id, out var user)": user is a value of the dictionary.
    /// For other methods the value of the out argument is unknown.
    /// </summary>
    private static Origin? GetOutArgumentOrigin(
        IOperation statementOperation,
        CodePosition statement,
        object variable)
    {
        var outArgument = statementOperation
            .DescendantsAndSelf()
            .OfType<IArgumentOperation>()
            .FirstOrDefault(argument =>
                argument.Parameter?.RefKind == RefKind.Out && AreSame(GetRelatedVariable(argument.Value), variable));

        return outArgument switch
        {
            null => null,
            { Parent: IInvocationOperation { TargetMethod.Name: "TryGetValue" } call }
                => Origin.Create(call.Instance, statement),
            _ => Origin.NotFound,
        };
    }

    /// <summary>
    /// True if the variable is a parameter whose method asks for the property with [IncludeRequired],
    /// which makes the caller of that method responsible for loading it.
    /// </summary>
    private bool HasIncludeRequiredAttribute(object variable)
        => variable is IParameterSymbol { ContainingSymbol: IMethodSymbol method } parameter &&
           AttributeHelper.MethodHasIncludeRequiredAttribute(method, parameter.Name, property);

    /// <summary>
    /// True if the variable is a parameter of the method, and not a local or a variable it captured.
    /// </summary>
    private static bool IsParameterOfMethod(object variable, IMethodSymbol method)
        => variable is IParameterSymbol parameter &&
           SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, method);

    /// <summary>
    /// True if the variable is the first parameter of the method that declares it.
    /// </summary>
    private static bool IsFirstParameter(object variable)
        => variable is IParameterSymbol { Ordinal: 0 };

    private static bool AreSame(object? first, object second)
    {
        if (first is ISymbol firstSymbol && second is ISymbol secondSymbol)
        {
            return SymbolEqualityComparer.Default.Equals(firstSymbol, secondSymbol);
        }

        return first is CaptureId && first.Equals(second);
    }
}
