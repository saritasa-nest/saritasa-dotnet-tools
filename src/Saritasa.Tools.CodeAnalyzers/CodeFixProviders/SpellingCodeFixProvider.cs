using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.CodeFixProviders;

/// <summary>
/// Quick-fix for <see cref="SpellingAnalyzer"/> that adds the reported word to <c>exclusions.txt</c>
/// under the configured <c>words</c> folder.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SpellingCodeFixProvider))]
[Shared]
public sealed class SpellingCodeFixProvider : CodeFixProvider
{
    private const string ExclusionsFileName = "exclusions.txt";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SpellingAnalyzer.DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

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

        var exclusions = TryFindExclusionsAdditionalDocument(
            context.Document.Project.Solution, context.Document.Project.AnalyzerOptions.AdditionalFiles);
        if (exclusions is null)
        {
            return Task.CompletedTask;
        }

        var title = $"Add '{word}' to exclusions";
        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                ct => AddWordToExclusionsAsync(context.Document.Project.Solution, exclusions, word!, ct),
                equivalenceKey: title),
            diagnostic);

        return Task.CompletedTask;
    }

    private static TextDocument? TryFindExclusionsAdditionalDocument(
        Solution solution,
        ImmutableArray<AdditionalText> additionalFiles)
    {
        // We can only apply a fix if exclusions.txt is included as an AdditionalFile in the solution.
        // (Roslyn code fix cannot reliably create arbitrary new files on disk.)
        var exclusionsPath = additionalFiles
            .Select(f => f.Path)
            .FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p) &&
                p.EndsWith(ExclusionsFileName, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(exclusionsPath))
        {
            return null;
        }

        return solution.Projects
            .SelectMany(p => p.AdditionalDocuments)
            .FirstOrDefault(d => string.Equals(d.FilePath, exclusionsPath, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Solution> AddWordToExclusionsAsync(
        Solution solution,
        TextDocument exclusionsDocument,
        string word,
        CancellationToken cancellationToken)
    {
        var text = await exclusionsDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var content = text.ToString();

        if (ContainsWord(content, word))
        {
            return solution;
        }

        // Add a trailing newline if missing, then append the word.
        var appended = content;
        if (appended.Length > 0 && !appended.EndsWith("\n", StringComparison.Ordinal))
        {
            appended += "\r\n";
        }

        appended += word + "\r\n";

        var newText = SourceText.From(appended, text.Encoding);
        return solution.WithAdditionalDocumentText(exclusionsDocument.Id, newText);
    }

    private static bool ContainsWord(string content, string word)
    {
        var lines = content.Split(["\r\n", "\n"], StringSplitOptions.None);
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
