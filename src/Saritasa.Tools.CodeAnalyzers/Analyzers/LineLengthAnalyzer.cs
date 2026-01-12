using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Reports warning when a source line exceeds the configured maximum length.
/// </summary>
/// <remarks>
/// Line length can be configured via .editorconfig option:
/// <c>max_line_length = 160</c>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LineLengthAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic identifier for long lines.
    /// </summary>
    private const string DiagnosticId = "STAN1001";
    private const string Category = "Style";

    /// <summary>
    /// Default value (used when no .editorconfig option is provided).
    /// According to
    /// <see href="https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#code-lines">5.2 code style</see>.
    /// </summary>
    /// <remarks>
    /// According to
    /// <see href="https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#code-lines">5.2 code style</see>.
    /// </remarks>
    private const int DefaultMaxLineLength = 130;

    private static readonly LocalizableString Title = "Line exceeds maximum length";
    private static readonly LocalizableString MessageFormat = "Line length is {0} characters, which exceeds the limit of {1}";
    private static readonly LocalizableString Description = "Keep lines within the configured maximum length.";

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
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var maxLineLength = GetMaxLineLength(context);

        var text = context.Tree.GetText(context.CancellationToken);
        foreach (var line in text.Lines)
        {
            var length = line.End - line.Start;
            if (length <= maxLineLength)
            {
                continue;
            }

            var location = Location.Create(context.Tree, line.Span);
            var diagnostic = Diagnostic.Create(Rule, location, length, maxLineLength);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static int GetMaxLineLength(SyntaxTreeAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);

        const string maxLineLengthOptionName = "max_line_length";

        var hasConfiguredMaxLength = options.TryGetValue(maxLineLengthOptionName, out var rawValue);
        if (hasConfiguredMaxLength && int.TryParse(rawValue, out var configured) && configured > 0)
        {
            return configured;
        }

        return DefaultMaxLineLength;
    }
}
