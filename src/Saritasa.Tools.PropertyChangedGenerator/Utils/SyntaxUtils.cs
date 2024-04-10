using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.PropertyChangedGenerator.Utils;

/// <summary>
/// Utils for <see cref="SyntaxNode"/>.
/// </summary>
internal static class SyntaxUtils
{
    /// <summary>
    /// Find syntax node parent of given type.
    /// </summary>
    /// <param name="root">Root syntax node.</param>
    /// <typeparam name="TNode">Syntax node type.</typeparam>
    /// <returns>Syntax node.</returns>
    public static TNode? FindParentOfType<TNode>(SyntaxNode root)
    {
        if (root.Parent is TNode node)
        {
            return node;
        }

        if (root.Parent is not null)
        {
            return FindParentOfType<TNode>(root.Parent);
        }

        return default;
    }
}
