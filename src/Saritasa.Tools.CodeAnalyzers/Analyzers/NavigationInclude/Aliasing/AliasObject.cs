using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// One object at runtime, named by where it comes from: a parameter, the operation that creates it, or the edge
/// whose value it was before the method assigned anything to that edge.
/// </summary>
/// <remarks>
/// Two objects with the same origin are the same object. An edge nothing assigned therefore gets the same object on
/// every branch, and the branches still agree where they meet.
/// </remarks>
internal sealed class AliasObject : IEquatable<AliasObject>
{
    private AliasObject(object origin)
    {
        Origin = origin;
    }

    /// <summary>
    /// Where the object comes from: an <see cref="IParameterSymbol"/>, an <see cref="IOperation"/> or an
    /// <see cref="Edge"/>.
    /// </summary>
    public object Origin { get; }

    /// <summary>
    /// The object the caller passed in a parameter.
    /// </summary>
    /// <param name="parameter">Parameter.</param>
    /// <returns>Object.</returns>
    public static AliasObject OfParameter(IParameterSymbol parameter) => new(parameter);

    /// <summary>
    /// The object an operation creates: <c>new</c>, <c>with</c>, a query, or a call to an <c>[Includes]</c> method.
    /// </summary>
    /// <param name="creation">Operation that creates the object.</param>
    /// <returns>Object.</returns>
    public static AliasObject CreatedBy(IOperation creation) => new(creation);

    /// <summary>
    /// The object an edge held before the method assigned anything to it, e.g. the Profile of a user parameter.
    /// </summary>
    /// <param name="edge">Edge.</param>
    /// <returns>Object.</returns>
    public static AliasObject InitialValueOf(Edge edge) => new(edge);

    /// <inheritdoc />
    public bool Equals(AliasObject? other)
        => other is not null && VariableComparer.Instance.Equals(Origin, other.Origin);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AliasObject);

    /// <inheritdoc />
    public override int GetHashCode() => VariableComparer.Instance.GetHashCode(Origin);
}
