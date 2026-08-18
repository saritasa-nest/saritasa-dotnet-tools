using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// Provides <see cref="DiagnosticDescriptor"/> definitions and identifiers for the <see cref="NavigationIncludeAnalyzer"/> rules.
/// </summary>
internal static class NavigationIncludeRulesProvider
{
    private const string Category = "Usage";

    /// <summary>
    /// Diagnostic identifier for INCL001.
    /// </summary>
    public const string RuleIncl1Id = "INCL001";

    /// <summary>
    /// Diagnostic identifier for INCL002.
    /// </summary>
    public const string RuleIncl2Id = "INCL002";

    /// <summary>
    /// Diagnostic identifier for INCL003.
    /// </summary>
    public const string RuleIncl3Id = "INCL003";

    #region INCL001

    private static readonly LocalizableString titleIncl1 =
        "Method parameter should require a navigation property";

    private static readonly LocalizableString messageFormatIncl1 =
        "Navigation property '{0}.{1}' is required, but not checked. Use [IncludeRequired(\"{2}\", \"{1}\")] to this method.";

    private static readonly LocalizableString descriptionIncl1 =
        "Navigation properties marked with [TrackIncludeRequired] must be declared via [IncludeRequired] on every " +
        "method that accesses them directly or propagates them to callee methods.";

    private static readonly DiagnosticDescriptor ruleIncl1 = new(
        RuleIncl1Id,
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
        "set the property in an object initializer, or annotate the source method with [Includes(\"{1}\")]";

    private static readonly LocalizableString descriptionIncl2 =
        "When passing a local variable to a method that requires a navigation property via [IncludeRequired], " +
        "the variable must have that property loaded via .Include(), set in an object initializer, or come from " +
        "a method decorated with [Includes(\"PropertyName\")].";

    private static readonly DiagnosticDescriptor ruleIncl2 = new(
        RuleIncl2Id,
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
        RuleIncl3Id,
        titleIncl3,
        messageFormatIncl3,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl3);

    #endregion

    /// <summary>
    /// All diagnostic descriptors registered by this analyzer.
    /// </summary>
    public static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =
        ImmutableArray.Create(ruleIncl1, ruleIncl2, ruleIncl3);

    /// <summary>
    /// Returns the diagnostic descriptor for the given rule id.
    /// </summary>
    /// <param name="ruleId">Rule id.</param>
    /// <returns>Diagnostic descriptor.</returns>
    public static DiagnosticDescriptor GetDiagnosticDescriptor(string ruleId)
    {
        return ruleId switch
        {
            RuleIncl1Id => ruleIncl1,
            RuleIncl2Id => ruleIncl2,
            RuleIncl3Id => ruleIncl3,
            _ => throw new ArgumentException("Unknown diagnostic rule id: " + ruleId)
        };
    }
}
