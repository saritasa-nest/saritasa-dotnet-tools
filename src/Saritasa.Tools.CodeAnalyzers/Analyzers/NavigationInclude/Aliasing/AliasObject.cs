using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// One object at runtime, named by where it comes from: the operation that creates it, or the slot whose value it
/// was before the method assigned anything to that slot.
/// </summary>
/// <remarks>
/// Two objects with the same origin are the same object. A slot nothing assigned therefore gets the same object on
/// every branch, and the branches still agree where they meet.
/// </remarks>
internal sealed class AliasObject : IEquatable<AliasObject>
{
    private AliasObject(object origin)
    {
        Origin = origin;
    }

    /// <summary>
    /// The object the method's variables belong to, so a variable is a slot like a property.
    /// </summary>
    public static AliasObject Frame { get; } = new(new object());

    /// <summary>
    /// Where the object comes from: an <see cref="IOperation"/> or a <see cref="Slot"/>.
    /// </summary>
    public object Origin { get; }

    /// <summary>
    /// The object an operation creates: <c>new</c>, <c>with</c>, a query, or a call to an <c>[Includes]</c> method.
    /// </summary>
    /// <param name="creation">Operation that creates the object.</param>
    /// <returns>Object.</returns>
    public static AliasObject CreatedBy(IOperation creation) => new(creation);

    /// <summary>
    /// The object a slot held before the method assigned anything to it, e.g. the object the caller passed in a
    /// parameter, or the Profile of that object.
    /// </summary>
    /// <param name="slot">Slot.</param>
    /// <returns>Object.</returns>
    public static AliasObject InitialValueOf(Slot slot) => new(slot);

    /// <inheritdoc />
    public bool Equals(AliasObject? other)
        => other is not null && VariableComparer.Instance.Equals(Origin, other.Origin);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AliasObject);

    /// <inheritdoc />
    public override int GetHashCode() => VariableComparer.Instance.GetHashCode(Origin);
}
