using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

internal static class NavigationInclncludeRulesProvider
{
    private const string Category = "Usage";
    public const string RuleIncl1Id = "INCL001";
    public const string RuleIncl2Id = "INCL002";

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

    public static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics = ImmutableArray.Create(ruleIncl1, ruleIncl2);

    public static DiagnosticDescriptor GetDiagnosticDescriptor(string ruleId)
    {
        return ruleId switch
        {
            RuleIncl1Id => ruleIncl1,
            RuleIncl2Id => ruleIncl2,
            _ => throw new ArgumentException("Unknown diagnostic rule id: " + ruleId)
        };
    }
}
