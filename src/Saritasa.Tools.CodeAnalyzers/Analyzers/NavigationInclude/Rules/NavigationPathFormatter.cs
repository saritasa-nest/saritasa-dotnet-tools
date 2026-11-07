using System.Text;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;

/// <summary>
/// Builds the arguments of the [IncludeRequired] or [Includes] a message suggests, with nameof so that they can be
/// copied as they are and survive a rename: <c>nameof(User.Settings), nameof(UserSettings.Profile)</c>.
/// </summary>
/// <remarks>
/// Each segment is named on the type that holds it, through a collection to its element: "Applications.Stats" of a
/// user gives <c>nameof(User.Applications), nameof(Application.Stats)</c>. A segment that cannot be named so, such
/// as a property of a generic type, makes the whole path a dotted string instead.
/// </remarks>
internal static class NavigationPathFormatter
{
    /// <summary>
    /// Arguments of [IncludeRequired]: <c>nameof(user), nameof(User.Settings), nameof(UserSettings.Profile)</c>.
    /// </summary>
    /// <param name="parameter">The parameter that has to ask for the path.</param>
    /// <param name="path">The path, counted from the parameter.</param>
    /// <returns>Arguments, separated by commas.</returns>
    public static string ForIncludeRequired(IParameterSymbol parameter, NavigationPath path)
    {
        var arguments = new StringBuilder()
            .AppendFormat("nameof({0})", parameter.Name);

        return AppendPath(arguments, parameter.Type, path).ToString();
    }

    /// <summary>
    /// Arguments of [Includes]: <c>nameof(User.Settings), nameof(UserSettings.Profile)</c>.
    /// </summary>
    /// <param name="entity">Type the path starts at; a container such as <c>Task&lt;List&lt;User&gt;&gt;</c> is
    /// read as what it holds. Null gives the dotted string.</param>
    /// <param name="path">The path.</param>
    /// <returns>Arguments, separated by commas.</returns>
    public static string ForIncludes(ITypeSymbol? entity, NavigationPath path)
        => AppendPath(new StringBuilder(), entity, path).ToString();

    /// <summary>
    /// Appends one nameof per segment, or the dotted string in place of all of them when one cannot be named.
    /// </summary>
    private static StringBuilder AppendPath(StringBuilder arguments, ITypeSymbol? entity, NavigationPath path)
    {
        // Where the path begins, after a prefix such as "nameof(user), " that is always kept.
        var start = arguments.Length;
        var type = CustomBridgeRules.EntityType.GetEntityType(entity);

        foreach (var segment in path.Segments)
        {
            // nameof needs a type it can name without type arguments, e.g. not Box<T>, and a property on it.
            if (type is not INamedTypeSymbol { IsGenericType: false } owner ||
                FindProperty(owner, segment) is not { } property)
            {
                // Drop the segments already written, so the path is not half nameof and half string.
                arguments.Length = start;
                return arguments.AppendFormat("{0}", path);
            }

            if (arguments.Length > start)
            {
                arguments.Append(", ");
            }

            arguments.AppendFormat(
                "nameof({0}.{1})",
                owner.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                property.Name);

            // The next segment is a property of what this one holds: the element for a collection.
            type = CustomBridgeRules.EntityType.GetEntityType(property.Type);
        }

        return arguments;
    }

    /// <summary>
    /// The property of the type or of one of its base types.
    /// </summary>
    private static IPropertySymbol? FindProperty(ITypeSymbol type, string name)
    {
        ITypeSymbol? current = type;

        while (current is not null)
        {
            var currentPropertySymbol = current
                .GetMembers(name)
                .OfType<IPropertySymbol>()
                .FirstOrDefault();

            if (currentPropertySymbol is { } property)
            {
                return property;
            }

            current = current.BaseType;
        }

        return null;
    }
}
