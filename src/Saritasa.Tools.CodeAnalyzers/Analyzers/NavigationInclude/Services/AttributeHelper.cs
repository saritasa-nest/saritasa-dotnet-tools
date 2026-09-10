using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

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
        out string? param,
        out string? includedProperty)
    {
        param = null;
        includedProperty = null;

        var name = attribute.AttributeClass?.Name;
        if (!string.Equals(name, nameof(IncludeRequiredAttribute), StringComparison.Ordinal))
        {
            return false;
        }

        if (attribute.ConstructorArguments.Length < 2)
        {
            return false;
        }

        param = attribute.ConstructorArguments[0].Value as string;
        includedProperty = attribute.ConstructorArguments[1].Value as string;

        return param is not null && includedProperty is not null;
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
    /// Returns true if method declares <see cref="IncludesAttribute"/>.
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
