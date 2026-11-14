using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// One place that holds a reference: a member of one object, e.g. (user, Profile). A variable is a member of
/// <see cref="AliasObject.Frame"/>, e.g. (Frame, user).
/// </summary>
/// <remarks>
/// The slot names the place, not what it holds: the objects it holds are its value in <see cref="AliasState"/>,
/// so an assignment replaces the value and the slot stays the same. Slots belong to objects, not to variables:
/// <c>profile.Settings</c> and <c>user.Profile.Settings</c> are two references, but when both reach the same
/// object they share one slot.
/// </remarks>
internal sealed class Slot : IEquatable<Slot>
{
    /// <summary>
    /// Initializes the slot.
    /// </summary>
    /// <param name="owner">Object the member belongs to.</param>
    /// <param name="member">Local, parameter, compiler temporary or <see cref="IPropertySymbol"/>.</param>
    public Slot(AliasObject owner, object member)
    {
        Owner = owner;
        Member = member;
    }

    /// <summary>
    /// Object the member belongs to.
    /// </summary>
    public AliasObject Owner { get; }

    /// <summary>
    /// Local, parameter, compiler temporary or <see cref="IPropertySymbol"/>.
    /// </summary>
    public object Member { get; }

    /// <summary>
    /// The slot of a variable.
    /// </summary>
    /// <param name="variable">Local, parameter or compiler temporary.</param>
    /// <returns>Slot.</returns>
    public static Slot OfVariable(object variable) => new(AliasObject.Frame, variable);

    /// <inheritdoc />
    public bool Equals(Slot? other)
        => other is not null &&
           Owner.Equals(other.Owner) &&
           VariableComparer.Instance.Equals(Member, other.Member);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Slot);

    /// <inheritdoc />
    public override int GetHashCode()
        => unchecked((Owner.GetHashCode() * 31) + VariableComparer.Instance.GetHashCode(Member));
}
