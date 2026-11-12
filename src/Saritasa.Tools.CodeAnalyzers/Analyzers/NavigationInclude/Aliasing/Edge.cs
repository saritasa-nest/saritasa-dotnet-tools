using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// One property of one object, e.g. (user, Profile). It points at the objects the property holds.
/// </summary>
/// <remarks>
/// Edges belong to objects, not to variables: <c>profile.Settings</c> and <c>user.Profile.Settings</c> are two
/// references, but when both reach the same object they share one edge.
/// </remarks>
internal sealed class Edge : IEquatable<Edge>
{
    /// <summary>
    /// Initializes the edge.
    /// </summary>
    /// <param name="owner">Object the property belongs to.</param>
    /// <param name="property">Property.</param>
    public Edge(AliasObject owner, IPropertySymbol property)
    {
        Owner = owner;
        Property = property;
    }

    /// <summary>
    /// Object the property belongs to.
    /// </summary>
    public AliasObject Owner { get; }

    /// <summary>
    /// Property.
    /// </summary>
    public IPropertySymbol Property { get; }

    /// <inheritdoc />
    public bool Equals(Edge? other)
        => other is not null &&
           Owner.Equals(other.Owner) &&
           SymbolEqualityComparer.Default.Equals(Property, other.Property);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Edge);

    /// <inheritdoc />
    public override int GetHashCode()
        => unchecked((Owner.GetHashCode() * 31) + SymbolEqualityComparer.Default.GetHashCode(Property));
}
