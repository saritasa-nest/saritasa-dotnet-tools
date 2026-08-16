using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

public class AttributeHelper
{
    public static bool HasTrackIncludeRequiredAttribute(ISymbol symbol)
    {
        return symbol
            .GetAttributes()
            .Any(a => string.Equals(
                a.AttributeClass?.Name,
                nameof(TrackIncludeRequiredAttribute),
                StringComparison.Ordinal));
    }

    public static bool TryGetIncludeRequiredArgs(
        AttributeData attr,
        out string? paramName,
        out string? propertyName)
    {
        paramName = null;
        propertyName = null;

        var name = attr.AttributeClass?.Name;
        if (!string.Equals(name, nameof(IncludeRequiredAttribute), StringComparison.Ordinal))
        {
            return false;
        }

        if (attr.ConstructorArguments.Length < 2)
        {
            return false;
        }

        paramName = attr.ConstructorArguments[0].Value as string;
        propertyName = attr.ConstructorArguments[1].Value as string;

        return paramName is not null && propertyName is not null;
    }

    public static bool MethodHasIncludeRequiredAttribute(
        IMethodSymbol method,
        string paramName,
        string propertyName)
    {
        foreach (var attr in method.GetAttributes())
        {
            if (!TryGetIncludeRequiredArgs(attr, out var attrParamName, out var attrPropertyName))
            {
                continue;
            }

            if (string.Equals(attrParamName, paramName, StringComparison.Ordinal) &&
                string.Equals(attrPropertyName, propertyName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static bool MethodHasIncludesAttribute(IMethodSymbol method, string propertyName)
    {
        foreach (var attr in method.GetAttributes())
        {
            if (TryGetIncludesArg(attr, out var attrPropertyName) &&
                string.Equals(attrPropertyName, propertyName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryGetIncludesArg(AttributeData attr, out string? propertyName)
    {
        propertyName = null;

        var name = attr.AttributeClass?.Name;
        if (!string.Equals(name, nameof(IncludesAttribute), StringComparison.Ordinal))
        {
            return false;
        }

        if (attr.ConstructorArguments.Length < 1)
        {
            return false;
        }

        propertyName = attr.ConstructorArguments[0].Value as string;
        return propertyName is not null;
    }
}
