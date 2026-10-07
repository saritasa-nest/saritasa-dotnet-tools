using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

/// <summary>
/// A point in execution inside a block: the first <see cref="StatementIndex"/> statements of
/// <see cref="Block"/> have already run, the rest have not.
/// </summary>
/// <remarks>
/// <see cref="Statement"/> runs next and <see cref="GetPreviousStatements"/> already ran, so one position
/// answers both "which statement is this?" and "what was written before it?". A block's last statement is its
/// branch value: an if/while condition or a returned value.
/// </remarks>
internal sealed class CodePosition
{
    /// <summary>
    /// Initializes the position.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <param name="statementIndex">Number of statements of the block that have already run.</param>
    public CodePosition(FlowGraph flowGraph, BasicBlock block, int statementIndex)
    {
        if (statementIndex < 0 || statementIndex > GetStatementCount(block))
        {
            throw new ArgumentOutOfRangeException(nameof(statementIndex), "The block has no such position.");
        }

        FlowGraph = flowGraph;
        Block = block;
        StatementIndex = statementIndex;
    }

    /// <summary>
    /// Graph the block belongs to.
    /// </summary>
    public FlowGraph FlowGraph { get; }

    /// <summary>
    /// Block.
    /// </summary>
    public BasicBlock Block { get; }

    /// <summary>
    /// Number of statements of the block that have already run.
    /// </summary>
    public int StatementIndex { get; }

    /// <summary>
    /// The statement that runs next. Null at the end of the block, where nothing runs next.
    /// </summary>
    public IOperation? Statement
        => StatementIndex < Block.Operations.Length
            ? Block.Operations[StatementIndex]
            : StatementIndex == Block.Operations.Length
                ? Block.BranchValue
                : null;

    /// <summary>
    /// True if the next statement is the value after "return" of a method or local function (not of a lambda).
    /// </summary>
    public bool IsReturnFromMethod =>
        !FlowGraph.IsLambda &&
        StatementIndex == Block.Operations.Length &&
        Block.BranchValue is not null &&
        Block.FallThroughSuccessor?.Semantics == ControlFlowBranchSemantics.Return;

    /// <summary>
    /// Returns the position at every statement of the block, in execution order, the branch value last.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <returns>Positions of the statements.</returns>
    public static IEnumerable<CodePosition> GetBlockStatements(FlowGraph flowGraph, BasicBlock block)
        => Enumerable
            .Range(0, GetStatementCount(block))
            .Select(statementIndex => new CodePosition(flowGraph, block, statementIndex));

    /// <summary>
    /// Returns the position at the end of the block, where a jump leaves it from.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <returns>Position.</returns>
    public static CodePosition GetPositionAtBlockEnd(FlowGraph flowGraph, BasicBlock block)
        => new(flowGraph, block, GetStatementCount(block));

    /// <summary>
    /// Returns every position of the block: before the first statement, between two statements,
    /// and at the end.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <returns>Positions.</returns>
    public static IEnumerable<CodePosition> GetEveryPositionInBlock(FlowGraph flowGraph, BasicBlock block)
        => Enumerable
            .Range(0, GetStatementCount(block) + 1)
            .Select(statementIndex => new CodePosition(flowGraph, block, statementIndex));

    /// <summary>
    /// Returns the statements of the block that already ran, the nearest first.
    /// </summary>
    /// <returns>Positions of the statements.</returns>
    public IEnumerable<CodePosition> GetPreviousStatements()
        => Enumerable
            .Range(0, StatementIndex)
            .Reverse()
            .Select(statementIndex => new CodePosition(FlowGraph, Block, statementIndex));

    /// <summary>
    /// Number of statements in the block, the branch value included.
    /// </summary>
    private static int GetStatementCount(BasicBlock block)
        => block.Operations.Length + (block.BranchValue is null ? 0 : 1);
}
