using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Warns when a class name contains plural noun (should be singular).
/// </summary>
/// <remarks>
/// According to
/// <see href="https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#naming">9.1 code style</see>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingularClassNameAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "STAN1003";
    private const string Category = "Naming";

    private static readonly LocalizableString Title = "Class name should use singular nouns";
    private static readonly LocalizableString MessageFormat
        = "Class name '{0}' contains plural noun; only last noun can be plural";

    private static readonly LocalizableString Description
        = "Class names should use singular nouns (e.g. 'UserController' instead of 'UsersController'). Last noun can be plural.";

    private static readonly ImmutableHashSet<string> AllowedPluralWords = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "News",
        "Settings",
        "Options",
        "Analytics",
        "Physics",
        "Mathematics",
        "Statics",
        "Dynamics",
        "Glass",
        "Class",
        "Gas",
        "Bus",
        "Cors");

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } typeSymbol)
        {
            return;
        }

        var name = typeSymbol.Name;
        if (!HasPluralSegment(name))
        {
            return;
        }

        var location = typeSymbol.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasPluralSegment(string name)
    {
        var segments = SplitPascalCase(name).ToList();
        if (segments.Count <= 1)
        {
            return false;
        }

        foreach (var segment in segments.Take(segments.Count - 1))
        {
            if (string.IsNullOrEmpty(segment))
            {
                continue;
            }

            if (AllowedPluralWords.Contains(segment))
            {
                continue;
            }

            if (segment.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (segment.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> SplitPascalCase(string identifier)
    {
        var builder = new StringBuilder();
        foreach (var ch in identifier)
        {
            if (char.IsUpper(ch) && builder.Length > 0)
            {
                yield return builder.ToString();
                builder.Clear();
            }

            builder.Append(ch);
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }
}
