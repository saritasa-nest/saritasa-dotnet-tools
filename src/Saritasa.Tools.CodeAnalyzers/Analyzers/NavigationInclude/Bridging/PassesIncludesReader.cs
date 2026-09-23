using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// Turns a <c>[PassesIncludes]</c> into a <see cref="Bridge"/>.
/// </summary>
/// <remarks>
/// The lookup and the rule reporting INCL006 must read the attribute the same way, so it is read here.
/// The two read in opposite directions: the attribute names where the entities come from, a bridge names
/// where the search goes next.
/// </remarks>
internal static class PassesIncludesReader
{
    /// <summary>
    /// True if the attribute is [PassesIncludes].
    /// </summary>
    /// <param name="attribute">Attribute.</param>
    /// <returns>True when it is one.</returns>
    public static bool IsPassesIncludes(AttributeData attribute)
        => string.Equals(
            attribute.AttributeClass?.Name,
            nameof(PassesIncludesAttribute),
            StringComparison.Ordinal);

    /// <summary>
    /// A [PassesIncludes] written on the member itself, which names its own parameters with nameof.
    /// </summary>
    /// <param name="attribute">Attribute.</param>
    /// <returns>Bridge, or null when the attribute is something else.</returns>
    public static Bridge? ReadMemberAttribute(AttributeData attribute)
        => IsPassesIncludes(attribute)
            ? CreateBridge(GetStringArgument(attribute, 0), attribute)
            : null;

    /// <summary>
    /// An <c>[assembly: PassesIncludes(typeof(SomeType), "Member", "source")]</c>, which names someone else's
    /// member by string.
    /// </summary>
    /// <param name="attribute">Attribute.</param>
    /// <returns>Bridge with the member it is about, or null when the attribute is something else.</returns>
    public static CustomBridge? ReadAssemblyAttribute(AttributeData attribute)
        => IsPassesIncludes(attribute) &&
           attribute.ConstructorArguments.Length >= 2 &&
           attribute.ConstructorArguments[0].Value is INamedTypeSymbol declaringType &&
           GetStringArgument(attribute, 1) is { } memberName
            ? new CustomBridge(
                declaringType.OriginalDefinition,
                memberName,
                CreateBridge(GetStringArgument(attribute, 2), attribute))
            : null;

    /// <summary>
    /// Turns what the attribute says into a bridge. It can name a result or a callback, never an out
    /// parameter.
    /// </summary>
    private static Bridge CreateBridge(string? landsOn, AttributeData attribute)
    {
        BridgeEnd from = GetNamedArgument(attribute, nameof(PassesIncludesAttribute.ToCallback)) is string callback
            ? new BridgeEnd.Parameter(
                callback,
                GetNamedArgument(attribute, nameof(PassesIncludesAttribute.ToCallbackParameter)) as int? ?? 0)
            : new BridgeEnd.Result();

        BridgeEnd to = string.IsNullOrEmpty(landsOn)
            ? new BridgeEnd.Instance()
            : new BridgeEnd.Parameter(landsOn!);

        return new Bridge(from, to);
    }

    private static object? GetNamedArgument(AttributeData attribute, string name)
        => attribute.NamedArguments
            .FirstOrDefault(argument => string.Equals(argument.Key, name, StringComparison.Ordinal))
            .Value.Value;

    private static string? GetStringArgument(AttributeData attribute, int index)
        => attribute.ConstructorArguments.Length > index
            ? attribute.ConstructorArguments[index].Value as string
            : null;
}
