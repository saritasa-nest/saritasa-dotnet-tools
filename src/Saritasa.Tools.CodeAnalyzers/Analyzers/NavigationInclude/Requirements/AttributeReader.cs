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
    /// Returns the parameter and the navigation path of every <see cref="IncludeRequiredAttribute"/> of the method.
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
            var path = ReadPath(attribute.ConstructorArguments.Skip(1));

            if (!string.IsNullOrEmpty(parameter) && path is not null)
            {
                yield return new IncludeRequirement(parameter!, path);
            }
        }
    }

    /// <summary>
    /// Returns true if the method asks for the path of the parameter with <see cref="IncludeRequiredAttribute"/>.
    /// A longer path asked for covers a shorter one: "Settings.Profile" covers "Settings".
    /// </summary>
    /// <param name="method">Method.</param>
    /// <param name="param">Parameter name.</param>
    /// <param name="path">Required navigation path.</param>
    /// <returns>True if the method asks for the path.</returns>
    public static bool MethodHasIncludeRequiredAttribute(
        IMethodSymbol method,
        string param,
        NavigationPath path)
        => GetIncludeRequirements(method).Any(requirement =>
            string.Equals(requirement.ParameterName, param, StringComparison.Ordinal) &&
            requirement.Path.Covers(path));

    /// <summary>
    /// Returns true if the method annotates <see cref="IncludesAttribute"/> for the path or a longer one.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <param name="path">Navigation path.</param>
    /// <returns>True if the method promises the path.</returns>
    public static bool MethodHasIncludesAttribute(IMethodSymbol method, NavigationPath path)
        => GetIncludes(method).Any(include => include.Path.Covers(path));

    /// <summary>
    /// Returns the paths of every <see cref="IncludesAttribute"/> of the method that the analyzer must
    /// check: all of them except those annotated with <c>Verify = false</c>.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <returns>Navigation paths.</returns>
    public static IEnumerable<NavigationPath> GetIncludesToVerify(IMethodSymbol method)
        => GetIncludes(method)
            .Where(include => include.Verify)
            .Select(include => include.Path);

    /// <summary>
    /// Returns the path and the <c>Verify</c> flag of every <see cref="IncludesAttribute"/> of the method.
    /// </summary>
    private static IEnumerable<(NavigationPath Path, bool Verify)> GetIncludes(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (!IsAttribute(attribute, nameof(IncludesAttribute)))
            {
                continue;
            }

            if (ReadPath(attribute.ConstructorArguments) is { } path)
            {
                yield return (path, IsVerifyEnabled(attribute));
            }
        }
    }

    /// <summary>
    /// Reads a path from constructor arguments: a single string such as "Profile" or "Settings.Profile", or the
    /// array of a <c>params string[]</c> constructor.
    /// </summary>
    private static NavigationPath? ReadPath(IEnumerable<TypedConstant> arguments)
    {
        var segments = new List<string?>();

        foreach (var argument in arguments)
        {
            if (argument.Kind == TypedConstantKind.Array)
            {
                foreach (var value in argument.Values)
                {
                    segments.Add(value.Value as string);
                }
            }
            else
            {
                segments.Add(argument.Value as string);
            }
        }

        return NavigationPath.Parse(segments);
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
