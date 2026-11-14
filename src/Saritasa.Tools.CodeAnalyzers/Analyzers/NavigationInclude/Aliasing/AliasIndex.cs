using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// The alias picture of one graph: one <see cref="AliasState"/> per block start, from which the picture at any
/// position is replayed.
/// </summary>
/// <remarks>
/// The forward pass reads the blocks in the order control reaches them. Where several blocks lead into one, their
/// pictures are combined; a loop leads back to an earlier block, so the pass repeats until no block start changes.
/// A lambda starts from the picture where it is created.
/// </remarks>
internal sealed class AliasIndex
{
    private readonly FlowGraph flowGraph;

    private readonly AliasEvaluator evaluator;

    // Null for a block control never reached.
    private readonly AliasState?[] blockStarts;

    // The picture at every position of a block, replayed on first use.
    private readonly Dictionary<BasicBlock, AliasState[]> blockStates = new();

    private AliasIndex(FlowGraph flowGraph)
    {
        this.flowGraph = flowGraph;
        evaluator = new AliasEvaluator(flowGraph);
        blockStarts = new AliasState?[flowGraph.Graph.Blocks.Length];
    }

    /// <summary>
    /// Builds the picture of every block start of the graph.
    /// </summary>
    /// <param name="flowGraph">Graph.</param>
    /// <returns>Index.</returns>
    public static AliasIndex Build(FlowGraph flowGraph)
    {
        var index = new AliasIndex(flowGraph);
        index.RunForwardPass();

        return index;
    }

    /// <summary>
    /// Returns the objects the reference may hold at the position, e.g. <c>user.Profile</c>.
    /// </summary>
    /// <param name="reference">Expression in the statement at the position.</param>
    /// <param name="position">Position; the statement there has not run yet.</param>
    /// <returns>Objects.</returns>
    public ObjectSet PointsTo(IOperation reference, CodePosition position)
        => evaluator.Evaluate(reference, GetState(position), position);

    /// <summary>
    /// Returns the picture at the position.
    /// </summary>
    /// <param name="position">Position; the statement there has not run yet.</param>
    /// <returns>Picture.</returns>
    public AliasState GetState(CodePosition position)
    {
        if (!blockStates.TryGetValue(position.Block, out var states))
        {
            states = Replay(position.Block);
            blockStates[position.Block] = states;
        }

        return states[position.StatementIndex];
    }

    private void RunForwardPass()
    {
        var blocks = flowGraph.Graph.Blocks;

        // The graph has no jumps for exceptions: a catch or finally starts with any picture of its try.
        var handlers = blocks
            .Select(block => (Block: block, Try: WritesWalker.FindHandledTryRegion(block)))
            .Where(handler => handler.Try is not null)
            .ToList();

        var pending = new Queue<BasicBlock>();
        Flow(blocks[0], GetInitialState(), pending);

        while (pending.Count > 0)
        {
            var block = pending.Dequeue();
            var states = Replay(block);
            var end = states[states.Length - 1];

            if (block.FallThroughSuccessor?.Destination is { } fallThrough)
            {
                Flow(fallThrough, end, pending);
            }

            if (block.ConditionalSuccessor?.Destination is { } conditional)
            {
                Flow(conditional, end, pending);
            }

            foreach (var handler in handlers)
            {
                if (block.Ordinal >= handler.Try!.FirstBlockOrdinal && block.Ordinal <= handler.Try.LastBlockOrdinal)
                {
                    Flow(handler.Block, states.Aggregate((first, second) => first.Combine(second)), pending);
                }
            }
        }
    }

    /// <summary>
    /// Adds the picture to what the block starts with, and reads the block again if that changed it.
    /// </summary>
    private void Flow(BasicBlock destination, AliasState state, Queue<BasicBlock> pending)
    {
        var current = blockStarts[destination.Ordinal];
        var combined = current is null ? state : current.Combine(state);

        if (current is not null && combined.Equals(current))
        {
            return;
        }

        blockStarts[destination.Ordinal] = combined;

        if (!pending.Contains(destination))
        {
            pending.Enqueue(destination);
        }
    }

    /// <summary>
    /// The picture before every statement of the block, and after the last one.
    /// </summary>
    private AliasState[] Replay(BasicBlock block)
    {
        var statements = CodePosition.GetBlockStatements(flowGraph, block).ToList();
        var states = new AliasState[statements.Count + 1];
        states[0] = blockStarts[block.Ordinal] ?? AliasState.Empty;

        for (var index = 0; index < statements.Count; index++)
        {
            states[index + 1] = evaluator.Apply(states[index], statements[index]);
        }

        return states;
    }

    /// <summary>
    /// A lambda sees the variables it captures as they are where it is created; any other body starts empty.
    /// </summary>
    private AliasState GetInitialState()
        => flowGraph.CreationStatement is { } creation
            ? creation.FlowGraph.Aliases.GetState(creation)
            : AliasState.Empty;
}
