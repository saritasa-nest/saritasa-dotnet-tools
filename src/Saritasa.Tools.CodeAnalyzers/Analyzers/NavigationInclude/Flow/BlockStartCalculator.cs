using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Calculates which navigation properties are loaded at the start of every block of a control flow graph.
/// </summary>
/// <remarks>
/// Loops are not supported yet. Blocks are ordered so that a block comes after the blocks that jump into it,
/// except for the jump from the end of a loop back to its start. Such jumps are ignored, so a loop body is
/// analyzed as if it runs once. To support loops, repeat the pass in
/// <see cref="CalculateTableAtStartOfEachBlock"/> until no table at a block end changes.
/// </remarks>
internal static class BlockStartCalculator
{
    /// <summary>
    /// Returns the table at the start of every block, indexed by <see cref="BasicBlock.Ordinal"/>.
    /// </summary>
    /// <param name="graph">Control flow graph of a method or lambda.</param>
    /// <param name="tableAtStart">Table at the start of the method or lambda.</param>
    /// <returns>Tables at block starts.</returns>
    public static LoadedProperties[] CalculateTableAtStartOfEachBlock(ControlFlowGraph graph, LoadedProperties tableAtStart)
    {
        var tablesAtBlockStart = new LoadedProperties[graph.Blocks.Length];
        var tablesAtBlockEnd = new LoadedProperties?[graph.Blocks.Length];

        foreach (var block in graph.Blocks)
        {
            var table = GetTableAtBlockStart(block, tableAtStart, tablesAtBlockEnd);
            tablesAtBlockStart[block.Ordinal] = table;
            tablesAtBlockEnd[block.Ordinal] = UpdateTableForAllStatements(block, table);
        }

        return tablesAtBlockStart;
    }

    private static LoadedProperties GetTableAtBlockStart(
        BasicBlock block,
        LoadedProperties tableAtStart,
        LoadedProperties?[] tablesAtBlockEnd)
    {
        if (block.Kind == BasicBlockKind.Entry)
        {
            return tableAtStart;
        }

        var incomingTables = new List<LoadedProperties>();
        foreach (var incomingJump in block.Predecessors)
        {
            // Null means the jump comes from a block that is not calculated yet: the end of a loop.
            var incomingTable = tablesAtBlockEnd[incomingJump.Source.Ordinal];
            if (incomingTable is not null)
            {
                incomingTables.Add(incomingTable);
            }
        }

        // No incoming tables (e.g. start of a catch or finally block): nothing is known to be loaded.
        return LoadedProperties.KeepOnlyWhatAllTablesAgreeOn(incomingTables);
    }

    private static LoadedProperties UpdateTableForAllStatements(BasicBlock block, LoadedProperties table)
    {
        foreach (var statement in block.Operations)
        {
            table = StatementEffects.UpdateTableForStatement(statement, table);
        }

        return table;
    }
}
