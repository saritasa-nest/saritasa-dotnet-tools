namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// One line of <see cref="BuiltInBridges"/>: a <see cref="Bridge"/> that ships with the analyzer, together
/// with the member it belongs to.
/// </summary>
/// <remarks>
/// A bridge is the rule alone, so everything naming the member it is about lives here, overloads included.
/// The declaring type is a metadata name, so the table can mention types the analyzer has no reference to;
/// <see cref="Bridges"/> looks it up once. What somebody else declares is a <see cref="CustomBridge"/>.
/// </remarks>
internal sealed class BuiltInBridge
{
    /// <summary>
    /// Initializes the line.
    /// </summary>
    /// <param name="declaringType">Metadata name of the type that declares the member.</param>
    /// <param name="memberName">Name of the method or property.</param>
    /// <param name="bridge">The move itself.</param>
    /// <param name="excludeOverloadsWithParameters">Parameters of the overloads this line does not describe.</param>
    public BuiltInBridge(
        string declaringType,
        string memberName,
        Bridge bridge,
        params string[] excludeOverloadsWithParameters)
    {
        DeclaringType = declaringType;
        MemberName = memberName;
        Bridge = bridge;
        ExcludeOverloadsWithParameters = excludeOverloadsWithParameters;
    }

    /// <summary>
    /// Metadata name of the type that declares the member, so that no reference to it is needed.
    /// </summary>
    public string DeclaringType { get; }

    /// <summary>
    /// Name of the method or property the bridge belongs to.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// The move itself.
    /// </summary>
    public Bridge Bridge { get; }

    /// <summary>
    /// Parameters whose presence means the overload hands back something else, so the line does not describe
    /// it. Empty when every overload behaves the same.
    /// </summary>
    /// <remarks>
    /// Neither the parameter count nor the types can separate these overloads: <c>Min(source, comparer)</c>
    /// and <c>Min(source, selector)</c> both take two, and <c>users.Min(u =&gt; u.Manager)</c> hands back a
    /// User just as <c>users.Min()</c> does. So the odd ones out are named instead.
    /// </remarks>
    public IReadOnlyList<string> ExcludeOverloadsWithParameters { get; }
}
