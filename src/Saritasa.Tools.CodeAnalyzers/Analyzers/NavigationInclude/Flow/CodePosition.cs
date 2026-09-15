using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// A statement in a graph: statement <see cref="StatementIndex"/> of <see cref="Block"/>.
/// When the position is a start point of a search, it means "right before this statement".
/// </summary>
/// <remarks>
/// Statement number <c>Block.Operations.Length</c> is the block's branch value (an if/while condition or
/// a returned value); it runs after the other statements. <c>Block.Operations.Length + 1</c> is the end of the block.
/// </remarks>
internal sealed class CodePosition
{
    /// <summary>
    /// Initializes the position.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <param name="statementIndex">Index of the statement.</param>
    public CodePosition(FlowGraph flowGraph, BasicBlock block, int statementIndex)
    {
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
    /// Index of the statement.
    /// </summary>
    public int StatementIndex { get; }

    /// <summary>
    /// The statement; null for the end of the block or a block without a branch value.
    /// </summary>
    public IOperation? Statement =>
        StatementIndex < Block.Operations.Length ? Block.Operations[StatementIndex]
        : StatementIndex == Block.Operations.Length ? Block.BranchValue
        : null;

    /// <summary>
    /// True if the statement is the value after "return" of a method or local function (not of a lambda).
    /// </summary>
    public bool IsReturnFromMethod =>
        FlowGraph.Lambda is null &&
        StatementIndex == Block.Operations.Length &&
        Block.BranchValue is not null &&
        Block.FallThroughSuccessor?.Semantics == ControlFlowBranchSemantics.Return;

    /// <summary>
    /// Creates the position after the whole block, including its branch value.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <returns>Position.</returns>
    public static CodePosition EndOfBlock(FlowGraph flowGraph, BasicBlock block)
        => new(flowGraph, block, block.Operations.Length + 1);

    /// <summary>
    /// Returns the statements of the block before this position, the nearest first.
    /// </summary>
    /// <returns>Positions of the statements.</returns>
    public IEnumerable<CodePosition> StatementsBefore()
        => Enumerable
            .Range(0, StatementIndex)
            .Reverse()
            .Select(statementIndex => new CodePosition(FlowGraph, Block, statementIndex))
            .Where(position => position.Statement is not null);
}
