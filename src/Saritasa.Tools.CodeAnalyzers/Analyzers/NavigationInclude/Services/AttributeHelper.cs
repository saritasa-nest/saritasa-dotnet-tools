using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Entities;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// Provides helpers for reading NavigationInclude-related attributes from Roslyn symbols.
/// </summary>
public class AttributeHelper
{
    /// <summary>
    /// Returns true if symbol is decorated with <see cref="TrackIncludeRequiredAttribute"/>.
    /// </summary>
    /// <param name="symbol">The symbol to inspect.</param>
    /// <returns>True if the attribute is present; otherwise false.</returns>
    public static bool HasTrackIncludeRequiredAttribute(ISymbol symbol)
    {
        return symbol
            .GetAttributes()
            .Any(a => string.Equals(
                a.AttributeClass?.Name,
                nameof(TrackIncludeRequiredAttribute),
                StringComparison.Ordinal));
    }

    /// <summary>
    /// Attempts to extract the parameter name and property name from an <see cref="IncludeRequiredAttribute"/>.
    /// </summary>
    /// <param name="attribute">The attribute data to inspect.</param>
    /// <param name="param">Receives the first constructor argument.</param>
    /// <param name="includedProperty">Receives the second constructor argument.</param>
    /// <returns>True if the attribute matches and both arguments are non-null.</returns>
    public static bool TryGetIncludeRequiredArgs(
        AttributeData attribute,
        out string param,
        out string includedProperty)
    {
        var name = attribute.AttributeClass?.Name;
        if (!string.Equals(name, nameof(IncludeRequiredAttribute), StringComparison.Ordinal))
        {
            param = string.Empty;
            includedProperty = string.Empty;
            return false;
        }

        if (attribute.ConstructorArguments.Length < 2)
        {
            param = string.Empty;
            includedProperty = string.Empty;
            return false;
        }

        param = attribute.ConstructorArguments[0].Value as string ?? string.Empty;
        includedProperty = attribute.ConstructorArguments[1].Value as string ?? string.Empty;

        return param != string.Empty && includedProperty != string.Empty;
    }

    /// <summary>
    /// Returns (parameter, property) of every <see cref="IncludeRequiredAttribute"/> of the method.
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <returns>Pairs of parameter name and required property name.</returns>
    public static IEnumerable<IncludeRequirement> GetIncludeRequirements(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (TryGetIncludeRequiredArgs(attribute, out var parameter, out var includedProperty))
            {
                yield return new IncludeRequirement(parameter, includedProperty);
            }
        }
    }

    /// <summary>
    /// Returns the properties of every <see cref="IncludesAttribute"/> of the method,
    /// except those declared with <c>Verify = false</c>.
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <returns>Property names the returned value must have loaded.</returns>
    public static IEnumerable<string> GetNotVerifiedIncludes(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (TryGetIncludesArg(attribute, out var includedProperty) && IsIncludesVerificationEnabled(attribute))
            {
                yield return includedProperty!;
            }
        }
    }

    /// <summary>
    /// Returns true if method declares <see cref="IncludeRequiredAttribute"/>.
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <param name="param">The parameter name to match.</param>
    /// <param name="includedProperty">The included property name to match.</param>
    /// <returns>True if a matching attribute is found; otherwise false.</returns>
    public static bool MethodHasIncludeRequiredAttribute(
        IMethodSymbol method,
        string param,
        string includedProperty)
    {
        foreach (var attr in method.GetAttributes())
        {
            if (!TryGetIncludeRequiredArgs(attr, out var attrParamName, out var attrPropertyName))
            {
                continue;
            }

            if (string.Equals(attrParamName, param, StringComparison.Ordinal) &&
                string.Equals(attrPropertyName, includedProperty, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if method declares <see cref="IncludesAttribute"/> for the property.
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <param name="includedProperty">The property name to match.</param>
    /// <returns>True if a matching attribute is found; otherwise false.</returns>
    public static bool MethodHasIncludesAttribute(IMethodSymbol method, string includedProperty)
    {
        foreach (var attr in method.GetAttributes())
        {
            if (TryGetIncludesArg(attr, out var attrPropertyName) &&
                string.Equals(attrPropertyName, includedProperty, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns false if the <see cref="IncludesAttribute"/> is declared with <c>Verify = false</c>.
    /// </summary>
    /// <param name="attribute">The <see cref="IncludesAttribute"/> data.</param>
    /// <returns>True if the analyzer should verify the method's return value.</returns>
    public static bool IsIncludesVerificationEnabled(AttributeData attribute)
    {
        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (string.Equals(namedArgument.Key, nameof(IncludesAttribute.Verify), StringComparison.Ordinal) &&
                namedArgument.Value.Value is false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Attempts to extract the property name from an <see cref="IncludesAttribute"/>.
    /// </summary>
    /// <param name="attribute">The attribute data to inspect.</param>
    /// <param name="includedProperty">Receives the first constructor argument.</param>
    /// <returns>True if the attribute matches and its argument is non-null.</returns>
    public static bool TryGetIncludesArg(AttributeData attribute, out string? includedProperty)
    {
        includedProperty = null;

        var name = attribute.AttributeClass?.Name;
        if (!string.Equals(name, nameof(IncludesAttribute), StringComparison.Ordinal))
        {
            return false;
        }

        if (attribute.ConstructorArguments.Length < 1)
        {
            return false;
        }

        includedProperty = attribute.ConstructorArguments[0].Value as string;
        return includedProperty is not null;
    }
}
