using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Reports warning when a word in identifiers, strings or comments is not found in the configured dictionaries.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpellingAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "STAN1004";
    private const string Category = "Spelling";

    private static readonly LocalizableString Title = "Possible spelling mistake";
    private static readonly LocalizableString MessageFormat = "Word '{0}' may be misspelled";
    private static readonly LocalizableString Description
        = "Verifies words in identifiers, strings and comments against provided dictionaries.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    internal const string DiagnosticPropertyWord = "word";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var dictionary = SpellDictionaryLoader.Load(compilationContext.Options.AdditionalFiles);
            if (dictionary.Count == 0)
            {
                return;
            }

            compilationContext.RegisterSyntaxTreeAction(c => AnalyzeSyntaxTree(c, dictionary));
        });
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context, ImmutableHashSet<string> dictionary)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        var descendantTokens = root.DescendantTokens(descendIntoTrivia: true);
        foreach (var token in descendantTokens)
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                CheckTrivia(context, dictionary, trivia);
            }

            foreach (var trivia in token.TrailingTrivia)
            {
                CheckTrivia(context, dictionary, trivia);
            }

            if (token.IsKind(SyntaxKind.IdentifierToken))
            {
                CheckIdentifierToken(context, dictionary, token);
            }

            if (token.IsKind(SyntaxKind.StringLiteralToken)
                || token.IsKind(SyntaxKind.CharacterLiteralToken)
                || token.IsKind(SyntaxKind.InterpolatedStringTextToken))
            {
                var textContent = token.ValueText.Length > 0 ? token.ValueText : token.Text;
                CheckTextToken(context, dictionary, textContent, token.Span);
            }
        }
    }

    private static void CheckIdentifierToken(
        SyntaxTreeAnalysisContext context,
        ImmutableHashSet<string> dictionary,
        SyntaxToken token)
    {
        if (dictionary.Contains(token.ValueText))
        {
            return;
        }

        foreach (var (word, offset) in SplitIdentifier(token.ValueText))
        {
            if (!ShouldCheckWord(dictionary, word))
            {
                continue;
            }

            var location = Location.Create(context.Tree, new TextSpan(token.Span.Start + offset, word.Length));
            Report(context, word, location);
        }
    }

    private static void CheckTextToken(
        SyntaxTreeAnalysisContext context,
        ImmutableHashSet<string> dictionary,
        string text,
        TextSpan span)
    {
        foreach (var (word, offset) in SplitByNonLetters(text))
        {
            if (!ShouldCheckWord(dictionary, word))
            {
                continue;
            }

            if (IsCamelCaseWord(word))
            {
                foreach (var (partWord, partOffset) in SplitCamelCase(word, offset))
                {
                    if (!ShouldCheckWord(dictionary, partWord))
                    {
                        continue;
                    }

                    var partLocation = Location.Create(context.Tree, new TextSpan(span.Start + partOffset, partWord.Length));
                    Report(context, partWord, partLocation);
                }

                continue;
            }

            var location = Location.Create(context.Tree, new TextSpan(span.Start + offset, word.Length));
            Report(context, word, location);
        }
    }

    private static void CheckTrivia(
        SyntaxTreeAnalysisContext context,
        ImmutableHashSet<string> dictionary,
        SyntaxTrivia trivia)
    {
        if (!IsCommentTrivia(trivia))
        {
            return;
        }

        var text = trivia.ToFullString();
        foreach (var (word, offset) in SplitByNonLetters(text))
        {
            if (!ShouldCheckWord(dictionary, word))
            {
                continue;
            }

            if (IsCamelCaseWord(word))
            {
                foreach (var (partWord, partOffset) in SplitCamelCase(word, offset))
                {
                    if (!ShouldCheckWord(dictionary, partWord))
                    {
                        continue;
                    }

                    var partLocation = Location
                        .Create(context.Tree, new TextSpan(trivia.FullSpan.Start + partOffset, partWord.Length));
                    Report(context, partWord, partLocation);
                }

                continue;
            }

            var location = Location.Create(context.Tree, new TextSpan(trivia.FullSpan.Start + offset, word.Length));
            Report(context, word, location);
        }
    }

    private static bool ShouldCheckWord(ImmutableHashSet<string> dictionary, string word)
    {
        if (dictionary.Count == 0)
        {
            return false;
        }

        if (word.Length <= 2)
        {
            return false;
        }

        return !dictionary.Contains(word);
    }

    private static void Report(SyntaxTreeAnalysisContext context, string word, Location location)
    {
        var diagnostic = Diagnostic.Create(
            Rule,
            location,
            properties: ImmutableDictionary<string, string?>.Empty.Add(DiagnosticPropertyWord, word),
            messageArgs: [word]);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsCommentTrivia(SyntaxTrivia trivia) => trivia.Kind() switch
    {
        SyntaxKind.SingleLineCommentTrivia => true,
        SyntaxKind.MultiLineCommentTrivia => true,
        SyntaxKind.SingleLineDocumentationCommentTrivia => true,
        SyntaxKind.MultiLineDocumentationCommentTrivia => true,
        SyntaxKind.DocumentationCommentExteriorTrivia => true,
        SyntaxKind.XmlText => true,
        _ => false
    };

    private static bool IsCamelCaseWord(string word)
    {
        var hasLower = false;
        var hasUpper = false;
        foreach (var ch in word)
        {
            if (!char.IsLetter(ch))
            {
                return false;
            }

            if (char.IsLower(ch))
            {
                hasLower = true;
            }
            else if (char.IsUpper(ch))
            {
                hasUpper = true;
            }

            if (hasLower && hasUpper)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<(string Word, int Offset)> SplitIdentifier(string identifier)
    {
        var segments = SplitByNonLetters(identifier);
        foreach (var segment in segments)
        {
            foreach (var part in SplitCamelCase(segment.Word, segment.Offset))
            {
                yield return part;
            }
        }
    }

    private static IEnumerable<(string Word, int Offset)> SplitByNonLetters(string text)
    {
        var start = -1;
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (char.IsLetter(ch))
            {
                if (start < 0)
                {
                    start = i;
                }

                continue;
            }

            if (start >= 0)
            {
                yield return (text.Substring(start, i - start), start);
                start = -1;
            }
        }

        if (start >= 0)
        {
            yield return (text.Substring(start, text.Length - start), start);
        }
    }

    private static IEnumerable<(string Word, int Offset)> SplitCamelCase(string word, int baseOffset)
    {
        var start = 0;
        for (var i = 1; i < word.Length; i++)
        {
            var current = word[i];
            var previous = word[i - 1];
            var nextIsLower = i + 1 < word.Length && char.IsLower(word[i + 1]);

            if (!char.IsLetter(current))
            {
                if (i > start)
                {
                    yield return (word.Substring(start, i - start), baseOffset + start);
                }

                start = i + 1;
                continue;
            }

            var boundaryFromLowerToUpper = char.IsLower(previous) && char.IsUpper(current);
            var boundaryFromAcronymToWord = char.IsUpper(previous) && char.IsUpper(current) && nextIsLower;

            if (boundaryFromLowerToUpper || boundaryFromAcronymToWord)
            {
                yield return (word.Substring(start, i - start), baseOffset + start);
                start = i;
            }
        }

        if (start < word.Length)
        {
            yield return (word.Substring(start, word.Length - start), baseOffset + start);
        }
    }

    private static class SpellDictionaryLoader
    {
        private const string ExclusionsFileName = "exclusions.txt";

        public static ImmutableHashSet<string> Load(IEnumerable<AdditionalText> files)
        {
            var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var path = file.Path;
                if (string.IsNullOrWhiteSpace(path) || !ContainsWordsMarker(path))
                {
                    continue;
                }

                if (path.EndsWith(ExclusionsFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var text = file.GetText();
                if (text is null)
                {
                    continue;
                }

                foreach (var line in text.Lines)
                {
                    AddWord(line.ToString(), builder);
                }
            }

            // Load exclusions last so they always win.
            foreach (var file in files)
            {
                var path = file.Path;
                if (string.IsNullOrWhiteSpace(path) || !ContainsWordsMarker(path))
                {
                    continue;
                }

                if (!path.EndsWith(ExclusionsFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var text = file.GetText();
                if (text is null)
                {
                    continue;
                }

                foreach (var line in text.Lines)
                {
                    AddWord(line.ToString(), builder);
                }
            }

            return builder.ToImmutable();
        }

        private static bool ContainsWordsMarker(string path) => path.Contains("words", StringComparison.OrdinalIgnoreCase);

        private static void AddWord(string? line, ImmutableHashSet<string>.Builder builder)
        {
            var word = line?.Trim();
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            builder.Add(word);
        }
    }
}
