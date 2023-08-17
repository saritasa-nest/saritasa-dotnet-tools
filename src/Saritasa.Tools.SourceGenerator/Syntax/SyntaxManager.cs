using Microsoft.CodeAnalysis;
using Saritasa.Tools.Abstractions.Syntax;
using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Abstractions.Syntax;

namespace Saritasa.Tools.SourceGenerator.Syntax;

/// <inheritdoc/>
public class SyntaxManager : ISyntaxManager
{
    private readonly SyntaxValueProvider syntaxProvider;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="syntaxProvider">Syntax provider.</param>
    public SyntaxManager(SyntaxValueProvider syntaxProvider)
    {
        this.syntaxProvider = syntaxProvider;
    }

    /// <summary>
    /// Get syntax nodes.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TSymbol">Symbol type.</typeparam>
    /// <param name="filter">Nodes filter.</param>
    /// <returns>Nodes.</returns>
    public IncrementalValuesProvider<SyntaxSymbolNode<TSymbol>> GetSymbols<TNode, TSymbol>(ISyntaxFilter<TNode>? filter = null)
        where TNode : SyntaxNode
        where TSymbol : class, ISymbol
    {
        var filterAction = GetNodeFilterAction(filter);
        return syntaxProvider.CreateSyntaxProvider(
            filterAction,
            (ctx, token) => new SyntaxSymbolNode<TSymbol>(ctx, token));
    }

    private static Func<SyntaxNode, CancellationToken, bool> GetNodeFilterAction<TNode>(ISyntaxFilter<TNode>? filter = null)
        where TNode : SyntaxNode
        => filter is null
            ? GetNodeFilterAction<TNode>
            : (node, token) => GetNodeFilterAction<TNode>(node, token, filter);

    private static bool GetNodeFilterAction<TNode>(SyntaxNode node, CancellationToken token)
        where TNode : SyntaxNode
        => node is TNode;

    private static bool GetNodeFilterAction<TNode>(SyntaxNode node, CancellationToken token, ISyntaxFilter<TNode> filter)
        where TNode : SyntaxNode
        => node is TNode @class && filter.IsValid(@class);
}
