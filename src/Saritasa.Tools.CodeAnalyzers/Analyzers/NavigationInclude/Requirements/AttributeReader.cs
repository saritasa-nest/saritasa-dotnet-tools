using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

/// <summary>
/// Reads the NavigationInclude attributes of a symbol.
/// </summary>
internal static class AttributeReader
{
    /// <summary>
    /// Returns true if the symbol is decorated with <see cref="TrackIncludeRequiredAttribute"/>.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <returns>True if the symbol has the attribute.</returns>
    public static bool HasTrackIncludeRequiredAttribute(ISymbol symbol)
        => symbol
            .GetAttributes()
            .Any(attribute => IsAttribute(attribute, nameof(TrackIncludeRequiredAttribute)));

    /// <summary>
    /// Returns the parameter and the property of every <see cref="IncludeRequiredAttribute"/> of the method.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <returns>Requirements.</returns>
    public static IEnumerable<IncludeRequirement> GetIncludeRequirements(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (!IsAttribute(attribute, nameof(IncludeRequiredAttribute)) ||
                attribute.ConstructorArguments.Length < 2)
            {
                continue;
            }

            var parameter = attribute.ConstructorArguments[0].Value as string;
            var property = attribute.ConstructorArguments[1].Value as string;

            if (!string.IsNullOrEmpty(parameter) && !string.IsNullOrEmpty(property))
            {
                yield return new IncludeRequirement(parameter!, property!);
            }
        }
    }

    /// <summary>
    /// Returns true if the method asks for the property of the parameter with
    /// <see cref="IncludeRequiredAttribute"/>.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <param name="param">Parameter name.</param>
    /// <param name="includedProperty">Navigation property name.</param>
    /// <returns>True if the method asks for the property.</returns>
    public static bool MethodHasIncludeRequiredAttribute(
        IMethodSymbol method,
        string param,
        string includedProperty)
        => GetIncludeRequirements(method).Any(requirement =>
            string.Equals(requirement.ParameterName, param, StringComparison.Ordinal) &&
            string.Equals(requirement.NavigationProperty, includedProperty, StringComparison.Ordinal));

    /// <summary>
    /// Returns true if the method annotates <see cref="IncludesAttribute"/> for the property.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <param name="includedProperty">Navigation property name.</param>
    /// <returns>True if the method promises the property.</returns>
    public static bool MethodHasIncludesAttribute(IMethodSymbol method, string includedProperty)
        => GetIncludes(method).Any(include =>
            string.Equals(include.Property, includedProperty, StringComparison.Ordinal));

    /// <summary>
    /// Returns the properties of every <see cref="IncludesAttribute"/> of the method that the analyzer must
    /// check: all of them except those annotated with <c>Verify = false</c>.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <returns>Navigation property names.</returns>
    public static IEnumerable<string> GetIncludesToVerify(IMethodSymbol method)
        => GetIncludes(method)
            .Where(include => include.Verify)
            .Select(include => include.Property);

    /// <summary>
    /// Returns the property and the <c>Verify</c> flag of every <see cref="IncludesAttribute"/> of the method.
    /// </summary>
    private static IEnumerable<(string Property, bool Verify)> GetIncludes(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (!IsAttribute(attribute, nameof(IncludesAttribute)) ||
                attribute.ConstructorArguments.Length < 1)
            {
                continue;
            }

            if (attribute.ConstructorArguments[0].Value is string property)
            {
                yield return (property, IsVerifyEnabled(attribute));
            }
        }
    }

    /// <summary>
    /// Returns false if the attribute sets <c>Verify = false</c>.
    /// </summary>
    private static bool IsVerifyEnabled(AttributeData attribute)
        => !attribute.NamedArguments.Any(argument =>
            string.Equals(argument.Key, nameof(IncludesAttribute.Verify), StringComparison.Ordinal) &&
            argument.Value.Value is false);

    /// <summary>
    /// Returns true if the attribute is of the type with the name.
    /// </summary>
    private static bool IsAttribute(AttributeData attribute, string attributeTypeName)
        => string.Equals(attribute.AttributeClass?.Name, attributeTypeName, StringComparison.Ordinal);
}
