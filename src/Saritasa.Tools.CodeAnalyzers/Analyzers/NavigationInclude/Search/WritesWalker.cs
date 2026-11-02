using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

/// <summary>
/// Answers one question: where was this variable written, above the place it is read.
/// </summary>
/// <remarks>
/// The walker reads code and decides nothing: it reports <see cref="Write"/>s and
/// <see cref="IncludeSearcher"/> says what they mean. It knows navigation properties only because
/// <c>user.Settings = x</c> and <c>user.Settings.Profile = x</c> write the start of the path "Settings.Profile"
/// asked about <c>user</c>, while an assignment of any other property does not, and the walk reads straight
/// past it.
/// A variable is a local, a parameter or a compiler temporary, see <see cref="GetVariable"/>.
/// </remarks>
internal sealed class WritesWalker
{
    /// <summary>
    /// Blocks already read for a variable and the path asked about it. Without this the walk would circle a loop
    /// forever. The path is part of the key because two branches can read the same variable for different paths,
    /// as in <c>flag ? user : user.Manager</c>; the second must not take the first one's answer for its own.
    /// </summary>
    private readonly HashSet<(BasicBlock Block, object Variable, NavigationPath Path)> visitedBlocks = new();

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
            IDeclarationExpressionOperation annotation => GetVariable(annotation.Expression), // "out var user"
            _ => null,
        };
    }

    /// <summary>
    /// Returns the writes the variable can come from, by reading the statements above the position upward.
    /// More than one write when several paths lead to the position.
    /// </summary>
    /// <param name="variable">Variable, see <see cref="GetVariable"/>.</param>
    /// <param name="position">Place the variable is read at.</param>
    /// <param name="path">Path asked about the variable. An assignment of its start on the variable is a write, and
    /// the walker remembers it with the blocks it has read.</param>
    /// <returns>Writes; never empty.</returns>
    public IEnumerable<Write> FindWrites(object variable, CodePosition position, NavigationPath path)
    {
        var writeInThisBlock = position.GetPreviousStatements()
            .Select(previous => GetWriteOrDefault(previous, variable, path))
            .FirstOrDefault(write => write is not null);

        if (writeInThisBlock is not null)
        {
            return [writeInThisBlock];
        }

        // No write in this block, so the value was already there when the block started.
        return FindWritesBeforeBlock(variable, position.FlowGraph, position.Block, path);
    }

    /// <summary>
    /// Returns the write the statement makes to the variable, or null when the statement leaves the variable
    /// alone and the walk has to keep reading upward.
    /// </summary>
    private Write? GetWriteOrDefault(CodePosition statement, object variable, NavigationPath path)
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

            // "user.Settings = ..." or "user.Settings.Profile = ..." for "Settings.Profile" (the compiler also
            // rewrites "new User { Settings = ... }" into this form). Another property says nothing, so the walk
            // keeps reading upward.
            ISimpleAssignmentOperation assignment when
                ReadAssignedCount(assignment, statement, variable, path) is { } assignedCount
                => new Write.MemberWritten(Value.Create(assignment.Value, statement), assignedCount, statement),

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
    /// For "user.Settings = ..." or "user.Settings.Profile = ...": how many properties at the start of the path
    /// asked about the variable were assigned. Null when the statement assigns something else.
    /// </summary>
    private int? ReadAssignedCount(
        ISimpleAssignmentOperation assignment,
        CodePosition statement,
        object variable,
        NavigationPath path)
    {
        var properties = new List<string>();
        var target = ReadThroughCapture(assignment.Target, statement, variable, path);

        while (target is IPropertyReferenceOperation { Instance: { } instance } reference)
        {
            properties.Insert(0, reference.Property.Name);
            target = ReadThroughCapture(instance, statement, variable, path);
        }

        return path.StartsWith(properties) && AreSame(GetVariable(target), variable)
            ? properties.Count
            : null;
    }

    /// <summary>
    /// The expression a compiler temporary was given above the statement, or the operation itself when it is not
    /// one or is the variable the walk is looking for. A right side that spans several statements makes the
    /// compiler capture the target first: "user.Profile = new UserProfile { ... }" becomes
    /// "#1 = user.Profile; #2 = new UserProfile(); ...; #1 = #2".
    /// </summary>
    private IOperation? ReadThroughCapture(
        IOperation operation,
        CodePosition statement,
        object variable,
        NavigationPath path)
    {
        // "#1.Settings = x" of "new User { Settings = x }" while looking for #1 itself.
        if (operation is not IFlowCaptureReferenceOperation reference || AreSame(reference.Id, variable))
        {
            return operation;
        }

        // The temporary is a variable like any other, so its write is found the same way. One that more than one
        // path writes cannot name a single target.
        var writes = FindWrites(reference.Id, statement, path).ToList();

        return writes.Count == 1 && writes[0] is Write.Written written ? written.Value.Operation : null;
    }

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
            { Parameter: { } parameter, Parent: IInvocationOperation call }
                => new Write.OutArgument(Value.Create(call, statement), parameter.Name),

            // An out argument of something that is not a call: there is nothing to follow.
            _ => new Write.Unreadable(),
        };
    }

    /// <summary>
    /// No statement of the block writes the variable, so it comes from every way control can enter the block.
    /// </summary>
    private IEnumerable<Write> FindWritesBeforeBlock(object variable, FlowGraph flowGraph, BasicBlock block, NavigationPath path)
    {
        if (block.Kind == BasicBlockKind.Entry)
        {
            // This is the first block, so there is nothing before it inside the body.
            return FindWritesBeforeBody(variable, flowGraph, path);
        }

        if (WasVisitedBefore(block, variable, path))
        {
            // A loop came back, or another branch already read the blocks above for the same path.
            return [new Write.NothingNew()];
        }

        // Unreachable code has no way in, and nothing is known about the variable there.
        return FindWaysIntoBlock(flowGraph, block)
            .SelectMany(wayIn => FindWrites(variable, wayIn, path))
            .DefaultIfEmpty(new Write.Unreadable());
    }

    /// <summary>
    /// Nothing in the body writes the variable, so it comes from outside: a parameter, or a capture.
    /// </summary>
    private IEnumerable<Write> FindWritesBeforeBody(object variable, FlowGraph flowGraph, NavigationPath path)
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
        return FindWrites(variable, lambdaCreation, path);
    }

    /// <summary>
    /// True if the walk already read this block for this variable and path, which happens when a loop leads back
    /// to it, or when two branches meet above it.
    /// </summary>
    private bool WasVisitedBefore(BasicBlock block, object variable, NavigationPath path)
        => !visitedBlocks.Add((block, variable, path));

    /// <summary>
    /// Where control can enter the block: every block that jumps to it and, for the first block of a catch,
    /// filter or finally, every place of the try it handles.
    /// </summary>
    /// <remarks>
    /// The graph has no jumps for exceptions, so a handler looks unreachable; an exception can leave the try
    /// anywhere, so every place in it is a way in.
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
    /// "try/catch" is TryAndCatch, "catch when" wraps the handler into FilterAndHandler, "try/finally" is
    /// TryAndFinally. The try region is always the first nested one.
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
