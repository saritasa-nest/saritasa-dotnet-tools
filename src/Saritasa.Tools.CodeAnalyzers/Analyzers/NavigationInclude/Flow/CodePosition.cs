using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// A statement in a graph: statement <see cref="StatementIndex"/> of <see cref="Block"/>.
/// </summary>
/// <remarks>
/// Statement number <c>Block.Operations.Length</c> is the block's branch value (an if/while condition or
/// a returned value); it runs after the other statements.
/// </remarks>
internal sealed class CodePosition
{
    /// <summary>
    /// Initializes the position.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <param name="statementIndex">Index of the statement.</param>
    public CodePosition(IFlowGraph flowGraph, BasicBlock block, int statementIndex)
    {
        FlowGraph = flowGraph;
        Block = block;
        StatementIndex = statementIndex;
        Statement = statementIndex < block.Operations.Length
            ? block.Operations[statementIndex]
            : statementIndex == block.Operations.Length && block.BranchValue is not null
                ? block.BranchValue
                : throw new ArgumentOutOfRangeException(nameof(statementIndex), "The block has no statement with this index.");
    }

    /// <summary>
    /// Graph the block belongs to.
    /// </summary>
    public IFlowGraph FlowGraph { get; }

    /// <summary>
    /// Block.
    /// </summary>
    public BasicBlock Block { get; }

    /// <summary>
    /// Index of the statement.
    /// </summary>
    public int StatementIndex { get; }

    /// <summary>
    /// The statement.
    /// </summary>
    public IOperation Statement { get; }

    /// <summary>
    /// True if the statement is the value after "return" of a method or local function (not of a lambda).
    /// </summary>
    public bool IsReturnFromMethod =>
        FlowGraph is MethodFlowGraph &&
        StatementIndex == Block.Operations.Length &&
        Block.FallThroughSuccessor?.Semantics == ControlFlowBranchSemantics.Return;

    /// <summary>
    /// Returns all statements of the block in execution order, the branch value last.
    /// </summary>
    /// <param name="flowGraph">Graph the block belongs to.</param>
    /// <param name="block">Block.</param>
    /// <returns>Positions of the statements.</returns>
    public static IEnumerable<CodePosition> AllInBlock(IFlowGraph flowGraph, BasicBlock block)
        => Enumerable
            .Range(0, block.Operations.Length + (block.BranchValue is null ? 0 : 1))
            .Select(statementIndex => new CodePosition(flowGraph, block, statementIndex));

    /// <summary>
    /// Returns the statements of the block before this one, the nearest first.
    /// </summary>
    /// <returns>Positions of the statements.</returns>
    public IEnumerable<CodePosition> StatementsBefore()
        => Enumerable
            .Range(0, StatementIndex)
            .Reverse()
            .Select(statementIndex => new CodePosition(FlowGraph, Block, statementIndex));
}
