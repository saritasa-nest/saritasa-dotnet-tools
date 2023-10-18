using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.PropertyChangedGenerator.Abstractions.Syntax;

/// <summary>
/// Syntax node.
/// </summary>
public interface ISyntaxNode<TSymbol>
    where TSymbol : class, ISymbol
{
    /// <summary>
    /// Node symbol.
    /// </summary>
    TSymbol? Symbol { get; }

    /// <summary>
    /// Semantic model.
    /// </summary>
    SemanticModel SemanticModel { get; }
}
