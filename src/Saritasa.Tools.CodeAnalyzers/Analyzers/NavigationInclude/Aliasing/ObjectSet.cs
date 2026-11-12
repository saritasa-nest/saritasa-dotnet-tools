using System.Collections.Immutable;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// The objects a variable or an edge may hold at one position.
/// </summary>
/// <remarks>
/// One object means sure. Several objects mean the branches that lead here disagree. Unknown means something the
/// analyzer cannot read gave the value, such as an unannotated call or a field.
/// </remarks>
internal sealed class ObjectSet : IEquatable<ObjectSet>
{
    private ObjectSet(ImmutableHashSet<AliasObject> objects, bool isUnknown)
    {
        Objects = objects;
        IsUnknown = isUnknown;
    }

    /// <summary>
    /// No object, as after <c>profile = null</c>.
    /// </summary>
    public static ObjectSet Empty { get; } = new(ImmutableHashSet<AliasObject>.Empty, isUnknown: false);

    /// <summary>
    /// Any object.
    /// </summary>
    public static ObjectSet Unknown { get; } = new(ImmutableHashSet<AliasObject>.Empty, isUnknown: true);

    /// <summary>
    /// Objects that are known to be possible.
    /// </summary>
    public ImmutableHashSet<AliasObject> Objects { get; }

    /// <summary>
    /// True if any other object is possible too.
    /// </summary>
    public bool IsUnknown { get; }

    /// <summary>
    /// True if exactly one object is possible.
    /// </summary>
    public bool IsSure => !IsUnknown && Objects.Count == 1;

    /// <summary>
    /// The set of one object.
    /// </summary>
    /// <param name="value">Object.</param>
    /// <returns>Sure set.</returns>
    public static ObjectSet Of(AliasObject value) => new(ImmutableHashSet.Create(value), isUnknown: false);

    /// <summary>
    /// Returns the objects possible in either set, as where two branches meet.
    /// </summary>
    /// <param name="other">Other set.</param>
    /// <returns>Combined set.</returns>
    public ObjectSet Union(ObjectSet other) => new(Objects.Union(other.Objects), IsUnknown || other.IsUnknown);

    /// <inheritdoc />
    public bool Equals(ObjectSet? other)
        => other is not null && IsUnknown == other.IsUnknown && Objects.SetEquals(other.Objects);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ObjectSet);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Objects have no order, so their hashes are combined with XOR.
        var hash = IsUnknown ? 1 : 0;

        foreach (var value in Objects)
        {
            hash ^= value.GetHashCode();
        }

        return hash;
    }
}
