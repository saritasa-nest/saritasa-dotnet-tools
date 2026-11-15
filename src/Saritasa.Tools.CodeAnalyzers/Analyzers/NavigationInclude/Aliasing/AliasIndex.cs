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
    /// Returns what the assignment at the statement does to the slots <c>variable.path</c> goes through, e.g.
    /// <c>profile.Settings = s</c> to <c>user.Profile.Settings</c> when profile is user.Profile.
    /// </summary>
    /// <param name="target">Assignment target.</param>
    /// <param name="statement">Position of the assignment.</param>
    /// <param name="variable">Variable the reference starts at.</param>
    /// <param name="path">Property names after the variable, in order.</param>
    /// <returns>The effect, and for <see cref="ChainEffect.Sure"/> how many properties of the path lead to the
    /// assigned slot, the assigned one included.</returns>
    public (ChainEffect Effect, int AssignedCount) FindChainEffect(
        IOperation target,
        CodePosition statement,
        object variable,
        IReadOnlyList<string> path)
    {
        if (evaluator.ReadAssignedProperty(target) is not { Instance: { } instance, Property: { IsIndexer: false } property })
        {
            return (ChainEffect.None, 0);
        }

        var state = GetState(statement);
        var receiver = evaluator.Evaluate(instance, state, statement);
        var owners = state.Read(Slot.OfVariable(variable));
        var ownerType = evaluator.GetVariableType(variable);
        var result = (ChainEffect.None, 0);

        for (var index = 0; index < path.Count; index++)
        {
            if (string.Equals(path[index], property.Name, StringComparison.Ordinal))
            {
                switch (Compare(receiver, owners))
                {
                    // One unsure slot is enough to make the whole answer unsure.
                    case ChainEffect.Maybe:
                        return (ChainEffect.Maybe, 0);

                    // The same object may come again further down, as in "user.Manager.Manager": the last one wins.
                    case ChainEffect.Sure:
                        result = (ChainEffect.Sure, index + 1);
                        break;
                }
            }

            // A collection on the way holds elements, which are new objects: no assignment reaches past it.
            if (ownerType is null || FindProperty(ownerType, path[index]) is not { } step)
            {
                break;
            }

            owners = state.ReadProperty(owners, step);
            ownerType = step.Type;
        }

        return result;
    }

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

    /// <summary>
    /// Whether the receiver of an assignment is the owner of a slot on the path.
    /// </summary>
    private static ChainEffect Compare(ObjectSet receiver, ObjectSet owners)
    {
        if (receiver.IsUnknown || owners.IsUnknown)
        {
            return ChainEffect.Maybe;
        }

        if (!receiver.Objects.Overlaps(owners.Objects))
        {
            return ChainEffect.None;
        }

        return receiver.IsSure && owners.IsSure ? ChainEffect.Sure : ChainEffect.Maybe;
    }

    /// <summary>
    /// The property of the type or of one of its base types.
    /// </summary>
    private static IPropertySymbol? FindProperty(ITypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.GetMembers(name).OfType<IPropertySymbol>().FirstOrDefault() is { } property)
            {
                return property;
            }
        }

        return null;
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
