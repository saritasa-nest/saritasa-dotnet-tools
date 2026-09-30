using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// One <c>[assembly: PassesIncludes(typeof(SomeType), "Member", "source")]</c>: a <see cref="Bridge"/>
/// somebody wrote for a member of a type they do not own.
/// </summary>
/// <remarks>
/// The type is already a symbol, since the attribute names it with <c>typeof</c>. It names the member by
/// string, so it speaks for every overload at once and can say nothing about them, where a
/// <see cref="BuiltInBridge"/> can. An attribute on the member itself needs none of this.
/// </remarks>
internal sealed class CustomBridge
{
    /// <summary>
    /// Initializes the bridge.
    /// </summary>
    /// <param name="declaringType">Type that declares the member.</param>
    /// <param name="memberName">Name of the method or property.</param>
    /// <param name="bridge">The move itself.</param>
    public CustomBridge(INamedTypeSymbol declaringType, string memberName, Bridge bridge)
    {
        DeclaringType = declaringType;
        MemberName = memberName;
        Bridge = bridge;
    }

    /// <summary>
    /// Type that declares the member.
    /// </summary>
    public INamedTypeSymbol DeclaringType { get; }

    /// <summary>
    /// Name of the method or property the bridge belongs to.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// The move itself.
    /// </summary>
    public Bridge Bridge { get; }
}
