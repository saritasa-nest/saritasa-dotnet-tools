using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Saritasa.Tools.CodeAnalyzers.Helpers;
using WeCantSpell.Hunspell;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Reports warning when a word in identifiers, strings or comments is not found in the configured dictionaries.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpellingAnalyzer : DiagnosticAnalyzer
{
    static SpellingAnalyzer()
    {
        AssemblyResolver.ResolveAssemblies();
    }

    /// <summary>
    /// Diagnostic identifier.
    /// </summary>
    public const string DiagnosticId = "STAN1004";

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
            var wordList = SpellChecker.CreateWordList(compilationContext.Options.AdditionalFiles);
            if (wordList is null)
            {
                return;
            }

            compilationContext.RegisterSyntaxTreeAction(c => AnalyzeSyntaxTree(c, wordList));
        });
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context, WordList wordList)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        var descendantTokens = root.DescendantTokens(descendIntoTrivia: true);
        foreach (var token in descendantTokens)
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                CheckTrivia(context, wordList, trivia);
            }

            foreach (var trivia in token.TrailingTrivia)
            {
                CheckTrivia(context, wordList, trivia);
            }

            if (token.IsKind(SyntaxKind.IdentifierToken))
            {
                CheckIdentifierToken(context, wordList, token);
            }

            if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.InterpolatedStringTextToken))
            {
                var text = token.ValueText.Length > 0 ? token.ValueText : token.Text;
                CheckTextToken(context, wordList, text, token.Span);
            }
        }
    }

    private static void CheckTrivia(
        SyntaxTreeAnalysisContext context,
        WordList wordList,
        SyntaxTrivia trivia)
    {
        if (!IsCommentTrivia(trivia))
        {
            return;
        }

        var text = trivia.ToFullString();
        var words = StringHelper.SplitByNonLetters(text);
        foreach (var (word, offset) in words)
        {
            if (!ShouldCheckWord(wordList, word))
            {
                continue;
            }

            if (StringHelper.IsCamelCaseWord(word))
            {
                var camelCaseWords = SplitCamelCase(word, offset);
                foreach (var (partWord, partOffset) in camelCaseWords)
                {
                    if (!ShouldCheckWord(wordList, partWord))
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

    private static void CheckIdentifierToken(
        SyntaxTreeAnalysisContext context,
        WordList wordList,
        SyntaxToken token)
    {
        var tokenText = token.ValueText;
        if (!ShouldCheckWord(wordList, tokenText))
        {
            return;
        }

        var words = SplitIdentifier(tokenText);
        foreach (var (word, offset) in words)
        {
            if (!ShouldCheckWord(wordList, word))
            {
                continue;
            }

            var location = Location.Create(context.Tree, new TextSpan(token.Span.Start + offset, word.Length));
            Report(context, word, location);
        }
    }

    private static void CheckTextToken(SyntaxTreeAnalysisContext context, WordList wordList, string text, TextSpan span)
    {
        var words = StringHelper.SplitByNonLetters(text);
        foreach (var (word, offset) in words)
        {
            if (!ShouldCheckWord(wordList, word))
            {
                continue;
            }

            if (StringHelper.IsCamelCaseWord(word))
            {
                foreach (var (partWord, partOffset) in SplitCamelCase(word, offset))
                {
                    if (!ShouldCheckWord(wordList, partWord))
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

    private static bool ShouldCheckWord(WordList wordList, string word)
    {
        if (word.Length <= 2)
        {
            return false;
        }

        return !wordList.Check(word);
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

    private static IEnumerable<(string Word, int Offset)> SplitIdentifier(string identifier)
    {
        var segments = StringHelper.SplitByNonLetters(identifier);
        foreach (var segment in segments)
        {
            foreach (var part in SplitCamelCase(segment.Word, segment.Offset))
            {
                yield return part;
            }
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
}
