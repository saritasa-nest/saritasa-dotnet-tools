using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Warns when a type name contains plural noun (should be singular).
/// </summary>
/// <remarks>
/// According to
/// <see href="https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#naming">9.1 code style</see>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingularTypeNameAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "STAN1003";
    private const string Category = "Naming";
    private const string AllowedPluralWordsOptionName = "dotnet_diagnostic.STAN1003.allowed_plural_words";

    private static readonly LocalizableString title = "Type names should use singular nouns";
    private static readonly LocalizableString messageFormat = "Type name '{0}' contains plural noun";

    private static readonly LocalizableString description
        = "Type names should use singular nouns (e.g. 'UserController' instead of 'UsersController').";

    /// <remarks>
    /// Additional allowed plural words can be configured via .editorconfig option:
    /// <c>dotnet_diagnostic.STAN1003.allowed_plural_words = Accounts, Items</c>.
    /// </remarks>
    private static readonly ImmutableHashSet<string> allowedPluralWords = ImmutableHashSet.Create(
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
        "Cors",
        "Status");

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticId,
        title,
        messageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: description);

    private static readonly List<TypeKind> typeKindsToAnalyze =
    [
        TypeKind.Class,
        TypeKind.Interface
    ];

    private static readonly List<string> keywordsToCheck =
    [
        "Controller",
        "Service"
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var userAllowedWords = GetUserAllowedPluralWords(compilationContext.Options);

            compilationContext.RegisterSymbolAction(
                symbolContext => AnalyzeNamedType(symbolContext, userAllowedWords),
                SymbolKind.NamedType);
        });
    }

    private static ImmutableHashSet<string> GetUserAllowedPluralWords(AnalyzerOptions analyzerOptions)
    {
        var options = analyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (!options.TryGetValue(AllowedPluralWordsOptionName, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
        {
            return ImmutableHashSet<string>.Empty;
        }

        var words = rawValue
            .Split(',')
            .Select(w => w.Trim())
            .Where(w => w.Length > 0);

        return ImmutableHashSet.CreateRange(StringComparer.Ordinal, words);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, ImmutableHashSet<string> userAllowedWords)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol || !typeKindsToAnalyze.Contains(typeSymbol.TypeKind))
        {
            return;
        }

        var name = typeSymbol.Name;

        var words = SplitPascalCase(name).ToList();
        var hasKeyword = words.Any(segment => keywordsToCheck.Contains(segment));
        if (!hasKeyword)
        {
            return;
        }

        if (!HasPluralWord(words, userAllowedWords))
        {
            return;
        }

        var location = typeSymbol.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(rule, location, name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasPluralWord(List<string> words, ImmutableHashSet<string> userAllowedWords)
    {
        if (words.Count <= 1)
        {
            return false;
        }

        foreach (var word in words.Take(words.Count - 1))
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (allowedPluralWords.Contains(word) || userAllowedWords.Contains(word))
            {
                continue;
            }

            if (word.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
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
