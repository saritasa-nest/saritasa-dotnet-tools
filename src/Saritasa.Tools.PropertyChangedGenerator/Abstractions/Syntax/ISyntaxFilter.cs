using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.PropertyChangedGenerator.Abstractions.Syntax;

/// <summary>
/// Syntax nodes filter.
/// </summary>
/// <typeparam name="TNode">Syntax node type.</typeparam>
public interface ISyntaxFilter<TNode>
    where TNode : SyntaxNode
{
    /// <summary>
    /// Apply filter to nodes.
    /// </summary>
    /// <param name="nodes">Nodes to filter.</param>
    IncrementalValuesProvider<TNode> Apply(IncrementalValuesProvider<TNode> nodes);

    /// <summary>
    /// Is node valid.
    /// </summary>
    /// <param name="node">Syntax node.</param>
    /// <returns>True, if valid.</returns>
    bool IsValid(TNode node);
}
