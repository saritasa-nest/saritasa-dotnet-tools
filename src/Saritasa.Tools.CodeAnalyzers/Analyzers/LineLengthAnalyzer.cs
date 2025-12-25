using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Reports warning when a source line exceeds the configured maximum length.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LineLengthAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic identifier for long lines.
    /// </summary>
    private const string DiagnosticId = "STAN1001";
    private const string Category = "Style";

    /// <summary>
    /// According to
    /// <see href="https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#code-lines">5.2 code style</see>.
    /// </summary>
    private const int MaxLineLength = 130;

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
        var text = context.Tree.GetText(context.CancellationToken);
        foreach (var line in text.Lines)
        {
            var length = line.End - line.Start;
            if (length <= MaxLineLength)
            {
                continue;
            }

            var location = Location.Create(context.Tree, line.Span);
            var diagnostic = Diagnostic.Create(Rule, location, length, MaxLineLength);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
