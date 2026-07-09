using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Saritasa.Tools.CodeAnalyzers.Analyzers;
using Saritasa.Tools.CodeAnalyzers.Helpers;

namespace Saritasa.Tools.CodeAnalyzers.CodeFixProviders;

/// <summary>
/// Quick-fix for <see cref="SpellingAnalyzer"/> that adds the reported word to <c>exclusions.txt</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SpellingCodeFixProvider))]
[Shared]
public sealed class SpellingCodeFixProvider : CodeFixProvider
{
    private const string CrlfNewLine = "\r\n";
    private const string CrNewLine = "\r";
    private const string LfNewLine = "\n";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SpellingAnalyzer.DiagnosticId);

    /// <inheritdoc />
    /// <remarks>
    /// Returning <c>null</c> intentionally disables the "Fix all occurrences" menu
    /// (fix in file / folder / solution). The exclusions word is added globally to the
    /// solution-level exclusions file, so per-scope batch fixing does not make sense here.
    /// </remarks>
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == SpellingAnalyzer.DiagnosticId);
        if (diagnostic is null)
        {
            return Task.CompletedTask;
        }

        if (!diagnostic.Properties.TryGetValue(SpellingAnalyzer.DiagnosticPropertyWord, out var word) ||
            string.IsNullOrWhiteSpace(word))
        {
            return Task.CompletedTask;
        }

        var exclusions = GetExclusions(
            context.Document.Project.Solution, context.Document.Project.AnalyzerOptions);
        if (exclusions is null)
        {
            return Task.CompletedTask;
        }

        var title = $"Add '{word}' to exclusions";
        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                cancellationToken
                    => AddWordToExclusions(context.Document.Project.Solution, exclusions, word!, cancellationToken),
                equivalenceKey: title),
            diagnostic);

        return Task.CompletedTask;
    }

    private static TextDocument? GetExclusions(Solution solution, AnalyzerOptions options)
    {
        // We can only apply a fix if exclusions file is included as an AdditionalFile in the solution.
        // (Roslyn code fix cannot reliably create arbitrary new files on disk.)
        var exclusionsFile = SpellChecker.TryGetUserExclusionsFile(options);
        if (exclusionsFile is null)
        {
            return null;
        }

        return solution.Projects
            .SelectMany(project => project.AdditionalDocuments)
            .FirstOrDefault(document => string.Equals(document.FilePath, exclusionsFile.Path, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Solution> AddWordToExclusions(
        Solution solution,
        TextDocument exclusionsDocument,
        string word,
        CancellationToken cancellationToken)
    {
        word = word.ToLowerInvariant();

        var text = await exclusionsDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var content = text.ToString();

        if (ContainsWord(content, word))
        {
            return solution;
        }

        var lineSeparator = GetLineSeparator(content);

        var appended = content;
        if (appended.Length > 0 && !appended.EndsWith(lineSeparator, StringComparison.Ordinal))
        {
            appended += lineSeparator;
        }

        appended += word + lineSeparator;

        var newText = SourceText.From(appended, text.Encoding);
        return solution.WithAdditionalDocumentText(exclusionsDocument.Id, newText);
    }

    private static string GetLineSeparator(string content)
    {
        if (content.Contains(CrlfNewLine, StringComparison.Ordinal))
        {
            return CrlfNewLine;
        }

        if (content.Contains(CrNewLine, StringComparison.Ordinal))
        {
            return CrNewLine;
        }

        return LfNewLine;
    }

    private static bool ContainsWord(string content, string word)
    {
        var lines = content.Split([CrlfNewLine, CrNewLine, LfNewLine], StringSplitOptions.None);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            if (string.Equals(trimmed, word, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
