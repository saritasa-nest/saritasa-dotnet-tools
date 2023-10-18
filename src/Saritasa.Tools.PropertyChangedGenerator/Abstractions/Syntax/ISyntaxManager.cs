using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Models;

namespace Saritasa.Tools.PropertyChangedGenerator.Abstractions.Syntax;

/// <summary>
/// Syntax manager.
/// </summary>
public interface ISyntaxManager
{
    /// <summary>
    /// Get syntax symbol nodes.
    /// </summary>
    /// <typeparam name="TNode">Syntax node type.</typeparam>
    /// <typeparam name="TSymbol">Symbol type.</typeparam>
    /// <param name="filter">Syntax node filter.</param>
    /// <returns>Syntax symbol nodes.</returns>
    IncrementalValuesProvider<SyntaxSymbolNode<TSymbol>> GetSymbols<TNode, TSymbol>(ISyntaxFilter<TNode>? filter = null)
        where TNode : SyntaxNode
        where TSymbol : class, ISymbol;
}
