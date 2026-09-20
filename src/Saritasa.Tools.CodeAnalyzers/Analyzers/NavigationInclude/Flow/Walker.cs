using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Answers one question: where was this variable written, above the place it is read.
/// </summary>
/// <remarks>
/// The walker reads code and decides nothing. It reports what it found as <see cref="Write"/>s, and
/// <see cref="IncludeSearch"/> says what that means for the navigation property. The one thing it is told is
/// the name of the property, because "user.Profile = x" is a write to the variable and any other property
/// is not; it still does not know what finding that write means.
/// A variable here is a local, a parameter or a compiler temporary (<see cref="CaptureId"/>),
/// see <see cref="GetVariable"/>.
/// </remarks>
internal sealed class Walker
{
    private readonly string property;

    /// <summary>
    /// Blocks already read for a variable. Around a loop the walk comes back to the same block; that path adds
    /// nothing new, and without this set the walk would never stop.
    /// </summary>
    private readonly HashSet<(BasicBlock Block, object Variable)> visitedBlocks = new();

    /// <summary>
    /// Initializes the walk.
    /// </summary>
    /// <param name="property">Navigation property the search is looking for.</param>
    public Walker(string property)
    {
        this.property = property;
    }

    /// <summary>
    /// Returns the local, parameter or compiler temporary the expression reads; null for anything else.
    /// </summary>
    /// <param name="operation">Expression.</param>
    /// <returns>Variable or null.</returns>
    public static object? GetVariable(IOperation? operation)
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
            IDeclarationExpressionOperation declaration => GetVariable(declaration.Expression), // "out var user"
            _ => null,
        };
    }

    /// <summary>
    /// Returns the writes the variable can come from, by reading the statements above the position upward.
    /// More than one write when several paths lead to the position.
    /// </summary>
    /// <param name="variable">Variable, see <see cref="GetVariable"/>.</param>
    /// <param name="position">Place the variable is read at.</param>
    /// <returns>Writes; never empty.</returns>
    public IEnumerable<Write> FindWrites(object variable, CodePosition position)
    {
        var writeInThisBlock = position.GetPreviousStatements()
            .Select(previous => GetWriteOrDefault(previous, variable))
            .FirstOrDefault(write => write is not null);

        if (writeInThisBlock is not null)
        {
            return [writeInThisBlock];
        }

        // No write in this block, so the value was already there when the block started.
        return FindWritesBeforeBlock(variable, position.FlowGraph, position.Block);
    }

    /// <summary>
    /// Returns the write the statement makes to the variable, or null when the statement leaves the variable
    /// alone and the walk has to keep reading upward.
    /// </summary>
    private Write? GetWriteOrDefault(CodePosition statement, object variable)
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
            ISimpleAssignmentOperation assignment when WritesVariable(assignment, variable)
                => new Write.Written(Value.Create(assignment.Value, statement)),

            // "user.Profile = ..." (the compiler also rewrites "new User { Profile = ... }" into this form).
            // Another property says nothing, so the walk keeps reading upward.
            ISimpleAssignmentOperation assignment when WritesPropertyOfVariable(assignment, variable)
                => new Write.MemberWritten(),

            // "#1 = ...": a temporary value the compiler creates for ?:, ??, object initializers and foreach.
            IFlowCaptureOperation capture when AreSame(capture.Id, variable)
                => new Write.Written(Value.Create(capture.Value, statement)),

            // "var (id, user) = pair" and "foreach (var (id, user) in dictionary)": user is a part of the pair.
            IDeconstructionAssignmentOperation deconstruction when DeconstructsIntoVariable(deconstruction, variable)
                => new Write.Written(Value.Create(deconstruction.Value, statement)),

            _ => ReadOutArgument(statementOperation, statement, variable),
        };
    }

    /// <summary>
    /// True for "user = ...", where the variable itself is assigned.
    /// </summary>
    private static bool WritesVariable(ISimpleAssignmentOperation assignment, object variable)
        => AreSame(GetVariable(assignment.Target), variable);

    /// <summary>
    /// True for "user.Profile = ...", where the property the search looks for is assigned on the variable.
    /// </summary>
    private bool WritesPropertyOfVariable(ISimpleAssignmentOperation assignment, object variable)
        => assignment.Target is IPropertyReferenceOperation reference &&
           AreSame(GetVariable(reference.Instance), variable) &&
           reference.Property.Name == property;

    /// <summary>
    /// True for "var (id, user) = pair", where the variable is one of the parts.
    /// </summary>
    private static bool DeconstructsIntoVariable(IDeconstructionAssignmentOperation deconstruction, object variable)
        => deconstruction.Target
            .DescendantsAndSelf()
            .Any(target => AreSame(GetVariable(target), variable));

    /// <summary>
    /// "dictionary.TryGetValue(id, out var user)": a call wrote the variable through an out argument.
    /// Null when the statement does not write the variable at all.
    /// </summary>
    private static Write? ReadOutArgument(IOperation statementOperation, CodePosition statement, object variable)
    {
        var outArgument = statementOperation
            .DescendantsAndSelf()
            .OfType<IArgumentOperation>()
            .FirstOrDefault(argument =>
                argument.Parameter?.RefKind == RefKind.Out && AreSame(GetVariable(argument.Value), variable));

        return outArgument switch
        {
            null => null,
            { Parent: IInvocationOperation call } => new Write.OutArgument(Value.Create(call, statement)),

            // An out argument of something that is not a call: there is nothing to follow.
            _ => new Write.Unreadable(),
        };
    }

    /// <summary>
    /// No statement of the block writes the variable, so its value comes from before the block: from every way
    /// control can enter it.
    /// </summary>
    private IEnumerable<Write> FindWritesBeforeBlock(object variable, FlowGraph flowGraph, BasicBlock block)
    {
        if (block.Kind == BasicBlockKind.Entry)
        {
            // This is the first block, so there is nothing before it inside the body.
            return FindWritesBeforeBody(variable, flowGraph);
        }

        if (WasVisitedBefore(block, variable))
        {
            return [new Write.NothingNew()];
        }

        // Unreachable code has no way in, and nothing is known about the variable there.
        return FindWaysIntoBlock(flowGraph, block)
            .SelectMany(wayIn => FindWrites(variable, wayIn))
            .DefaultIfEmpty(new Write.Unreadable());
    }

    /// <summary>
    /// Nothing inside the body of the method, local function or lambda writes the variable, so its value comes
    /// from outside: it is a parameter, or a variable a lambda captured.
    /// </summary>
    private IEnumerable<Write> FindWritesBeforeBody(object variable, FlowGraph flowGraph)
    {
        if (flowGraph.CreationStatement is not { } lambdaCreation)
        {
            return [variable is IParameterSymbol parameter
                ? new Write.MethodParameter(parameter)
                : new Write.NeverWritten()];
        }

        if (flowGraph.Lambda is { } lambda && IsParameterOfBody(variable, flowGraph.Method))
        {
            return [new Write.LambdaParameter((IParameterSymbol)variable, lambda, lambdaCreation)];
        }

        // A variable captured by the lambda: keep reading before the statement that creates the lambda.
        return FindWrites(variable, lambdaCreation);
    }

    /// <summary>
    /// True if the walk already read this block for this variable, which happens when a loop leads back to it.
    /// </summary>
    private bool WasVisitedBefore(BasicBlock block, object variable)
        => !visitedBlocks.Add((block, variable));

    /// <summary>
    /// Places control can enter the block from: the end of every block that jumps to it and, for the first block
    /// of a catch, filter or finally, every place of the try block it handles.
    /// </summary>
    /// <remarks>
    /// The graph has no jumps for exceptions, so a handler looks unreachable. An exception can leave the try block
    /// after any of its statements, which is why every place of it is a way into the handler.
    /// </remarks>
    private static IEnumerable<CodePosition> FindWaysIntoBlock(FlowGraph flowGraph, BasicBlock block)
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
    /// True if the variable is a parameter of the body being read, and not a local or a variable it captured.
    /// </summary>
    private static bool IsParameterOfBody(object variable, IMethodSymbol body)
        => variable is IParameterSymbol parameter &&
           SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, body);

    private static bool AreSame(object? first, object second)
    {
        if (first is ISymbol firstSymbol && second is ISymbol secondSymbol)
        {
            return SymbolEqualityComparer.Default.Equals(firstSymbol, secondSymbol);
        }

        return first is CaptureId && first.Equals(second);
    }
}
