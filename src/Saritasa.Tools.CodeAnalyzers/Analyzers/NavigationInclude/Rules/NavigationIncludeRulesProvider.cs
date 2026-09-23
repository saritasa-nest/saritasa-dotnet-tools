using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;

/// <summary>
/// Provides <see cref="DiagnosticDescriptor"/> definitions and identifiers for the <see cref="NavigationIncludeAnalyzer"/> rules.
/// </summary>
internal static class NavigationIncludeRulesProvider
{
    private const string Category = "Usage";

    /// <summary>
    /// Diagnostic identifier for INCL001.
    /// </summary>
    public const string Incl1IdAddIncludeRequiredForParameter = "INCL001";

    /// <summary>
    /// Diagnostic identifier for INCL002.
    /// </summary>
    public const string Incl2IdArgumentDoesntIncludeNavigationProperty = "INCL002";

    /// <summary>
    /// Diagnostic identifier for INCL003.
    /// </summary>
    public const string Incl3IdMethodResultDoesntIncludeNavigationProperty = "INCL003";

    /// <summary>
    /// Diagnostic identifier for INCL004.
    /// </summary>
    public const string Incl4IdCannotCheckNavigationProperty = "INCL004";

    /// <summary>
    /// Diagnostic identifier for INCL005.
    /// </summary>
    public const string Incl5IdBridgeNamesNothing = "INCL005";

    /// <summary>
    /// Diagnostic identifier for INCL006.
    /// </summary>
    public const string Incl6IdBridgeIsNotAllowed = "INCL006";

    #region INCL001

    private static readonly LocalizableString titleIncl1 =
        "Method parameter should require a navigation property";

    private static readonly LocalizableString messageFormatIncl1 =
        "Navigation property '{0}.{1}' is required, but not checked. Use [IncludeRequired(\"{2}\", \"{1}\")] to this method.";

    private static readonly LocalizableString descriptionIncl1 =
        "Navigation properties marked with [TrackIncludeRequired] must be declared via [IncludeRequired] on every " +
        "method that accesses them directly or propagates them to callee methods.";

    private static readonly DiagnosticDescriptor ruleIncl1 = new(
        Incl1IdAddIncludeRequiredForParameter,
        titleIncl1,
        messageFormatIncl1,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl1);

    #endregion

    #region INCL002

    private static readonly LocalizableString titleIncl2 =
        "Local variable does not set the required navigation property";

    private static readonly LocalizableString messageFormatIncl2 =
        "Navigation property '{0}.{1}' is not loaded for local variable '{2}'; use .Include(x => x.{1}), " +
        "set the property in an object initializer, annotate the source method with [Includes(\"{1}\")], " +
        "or with [PassesIncludes] if it hands back the entities it was given";

    private static readonly LocalizableString descriptionIncl2 =
        "When passing a local variable to a method that requires a navigation property via [IncludeRequired], " +
        "the variable must have that property loaded via .Include(), set in an object initializer, or come from " +
        "a method decorated with [Includes(\"PropertyName\")].";

    private static readonly DiagnosticDescriptor ruleIncl2 = new(
        Incl2IdArgumentDoesntIncludeNavigationProperty,
        titleIncl2,
        messageFormatIncl2,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl2);

    #endregion

    #region INCL003

    private static readonly LocalizableString titleIncl3 =
        "Method declares [Includes] but return value does not load the required navigation property";

    private static readonly LocalizableString messageFormatIncl3 =
        "Method declares [Includes(\"{0}\")] but the returned value does not load navigation property '{0}'; " +
        "add .Include(x => x.{0}) to the query, set the property in an object initializer, or remove [Includes(\"{0}\")]";

    private static readonly LocalizableString descriptionIncl3 =
        "A method annotated with [Includes(\"PropertyName\")] promises its return value has the named navigation " +
        "property loaded. The analyzer verifies this by checking the return expression for .Include(), an object " +
        "initializer that sets the property, or a source method annotated with [Includes].";

    private static readonly DiagnosticDescriptor ruleIncl3 = new(
        Incl3IdMethodResultDoesntIncludeNavigationProperty,
        titleIncl3,
        messageFormatIncl3,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl3);

    #endregion

    #region INCL004

    private static readonly LocalizableString titleIncl4 =
        "Cannot check whether the navigation property is loaded";

    private static readonly LocalizableString messageFormatIncl4 =
        "Cannot check whether navigation property '{1}.{2}' is loaded for '{3}': the analyzer cannot read {0}. " +
        "Mark it with [PassesIncludes] if it hands back the entities it was given.";

    private static readonly LocalizableString descriptionIncl4 =
        "The analyzer follows a value back to the query it came from. When the path goes through a method or a " +
        "type it cannot read, it cannot say whether the navigation property is loaded. This is not a mistake in " +
        "the code: it is a gap in what the analyzer knows.";

    private static readonly DiagnosticDescriptor ruleIncl4 = new(
        Incl4IdCannotCheckNavigationProperty,
        titleIncl4,
        messageFormatIncl4,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl4);

    #endregion

    #region INCL005

    private static readonly LocalizableString titleIncl5 =
        "[PassesIncludes] names a method that does not exist";

    private static readonly LocalizableString messageFormatIncl5 =
        "[PassesIncludes] names '{0}', which does not exist, so the declaration has no effect";

    private static readonly LocalizableString descriptionIncl5 =
        "An assembly-level [PassesIncludes] names a method of another type by string. When that method is " +
        "renamed or removed, the declaration stops matching anything and the analyzer quietly stops following it.";

    private static readonly DiagnosticDescriptor ruleIncl5 = new(
        Incl5IdBridgeNamesNothing,
        titleIncl5,
        messageFormatIncl5,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl5);

    #endregion

    #region INCL006

    private static readonly LocalizableString titleIncl6 =
        "[PassesIncludes] is not allowed on this method";

    private static readonly LocalizableString messageFormatIncl6 =
        "[PassesIncludes] on '{0}' is ignored: {1}";

    private static readonly LocalizableString descriptionIncl6 =
        "A [PassesIncludes] promises that a method hands back the same entity objects it was given. Only a " +
        "static method can be held to that: an instance method may change what it was called on before " +
        "handing anything back, and nothing the analyzer reads would say so. A declaration that breaks the " +
        "rule is ignored rather than trusted, which is reported here so that it is not mistaken for a bridge " +
        "that works.";

    private static readonly DiagnosticDescriptor ruleIncl6 = new(
        Incl6IdBridgeIsNotAllowed,
        titleIncl6,
        messageFormatIncl6,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl6);

    #endregion

    /// <summary>
    /// All diagnostic descriptors registered by this analyzer.
    /// </summary>
    public static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =
        ImmutableArray.Create(ruleIncl1, ruleIncl2, ruleIncl3, ruleIncl4, ruleIncl5, ruleIncl6);

    /// <summary>
    /// Returns the diagnostic descriptor for the given rule id.
    /// </summary>
    /// <param name="ruleId">Rule id.</param>
    /// <returns>Diagnostic descriptor.</returns>
    public static DiagnosticDescriptor GetDiagnosticDescriptor(string ruleId)
    {
        return ruleId switch
        {
            Incl1IdAddIncludeRequiredForParameter => ruleIncl1,
            Incl2IdArgumentDoesntIncludeNavigationProperty => ruleIncl2,
            Incl3IdMethodResultDoesntIncludeNavigationProperty => ruleIncl3,
            Incl4IdCannotCheckNavigationProperty => ruleIncl4,
            Incl5IdBridgeNamesNothing => ruleIncl5,
            Incl6IdBridgeIsNotAllowed => ruleIncl6,
            _ => throw new ArgumentException("Unknown diagnostic rule id: " + ruleId)
        };
    }
}
