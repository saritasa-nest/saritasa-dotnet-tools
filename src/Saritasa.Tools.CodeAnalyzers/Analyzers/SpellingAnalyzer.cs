using System.Collections.Immutable;
using System.Text.RegularExpressions;
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
#if DEBUG
    static SpellingAnalyzer()
    {
        AssemblyResolver.ResolveAssemblies();
    }
#endif

    /// <summary>
    /// Diagnostic identifier.
    /// </summary>
    public const string DiagnosticId = "STAN1004";

    private const string Category = "Spelling";

    private static readonly LocalizableString title = "Typo";
    private static readonly LocalizableString messageFormat = "Word '{0}' has a typo";
    private static readonly LocalizableString description
        = "Verifies words in identifiers, strings and comments against provided dictionaries.";

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticId,
        title,
        messageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: description);

    internal const string DiagnosticPropertyWord = "word";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var wordList = SpellChecker.CreateWordList(compilationContext.Options.AdditionalFiles);
            var compilation = compilationContext.Compilation;

            compilationContext.RegisterSemanticModelAction(c => AnalyzeSemanticModel(c, wordList, compilation));
        });
    }

    private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context, WordList wordList, Compilation compilation)
    {
        var semanticModel = context.SemanticModel;
        var tree = semanticModel.SyntaxTree;
        var root = tree.GetRoot(context.CancellationToken);

        var descendantTokens = root.DescendantTokens(descendIntoTrivia: true);
        foreach (var token in descendantTokens)
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                CheckTrivia(context, tree, wordList, trivia);
            }

            foreach (var trivia in token.TrailingTrivia)
            {
                CheckTrivia(context, tree, wordList, trivia);
            }

            if (token.IsKind(SyntaxKind.IdentifierToken))
            {
                CheckIdentifierToken(context, tree, wordList, token, semanticModel, compilation);
            }

            if (IsString(token))
            {
                CheckText(context, tree, wordList, token.Text, token.Span.Start);
            }
        }
    }

    private static void CheckTrivia(
        SemanticModelAnalysisContext context,
        SyntaxTree tree,
        WordList wordList,
        SyntaxTrivia trivia)
    {
        if (!IsCommentTrivia(trivia))
        {
            return;
        }

        var text = trivia.ToFullString();
        CheckText(context, tree, wordList, text, trivia.FullSpan.Start);
    }

    private static void CheckIdentifierToken(
        SemanticModelAnalysisContext context,
        SyntaxTree tree,
        WordList wordList,
        SyntaxToken token,
        SemanticModel semanticModel,
        Compilation compilation)
    {
        if (IsExternalSymbol(token, semanticModel, compilation))
        {
            return;
        }

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

            var location = Location.Create(tree, new TextSpan(token.Span.Start + offset, word.Length));
            Report(context, word, location);
        }
    }

    private static bool IsExternalSymbol(SyntaxToken token, SemanticModel semanticModel, Compilation compilation)
    {
        var node = token.Parent;
        if (node == null)
        {
            return false;
        }

        var symbol = semanticModel.GetSymbolInfo(node).Symbol ?? semanticModel.GetDeclaredSymbol(node);
        if (symbol == null)
        {
            return false;
        }

        var containingAssembly = symbol.ContainingAssembly;
        if (containingAssembly == null)
        {
            return false;
        }

        return !SymbolEqualityComparer.Default.Equals(containingAssembly, compilation.Assembly);
    }

    private static bool IsString(SyntaxToken token) =>
        token.IsKind(SyntaxKind.StringLiteralToken)
        || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
        || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
        || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken);

    private static void CheckText(
        SemanticModelAnalysisContext context,
        SyntaxTree tree,
        WordList wordList,
        string text,
        int baseOffset)
    {
        var parts = SplitByWhitespace(text);

        foreach (var (part, partOffset) in parts)
        {
            if (IsGuid(part) || IsUrl(part))
            {
                continue;
            }

            var words = StringHelper.SplitByNonLetters(part);
            foreach (var (word, wordOffset) in words)
            {
                if (!ShouldCheckWord(wordList, word))
                {
                    continue;
                }

                if (StringHelper.IsCamelCaseWord(word))
                {
                    var camelCaseWords = StringHelper.SplitCamelCase(word, partOffset + wordOffset);
                    foreach (var (camelWord, camelOffset) in camelCaseWords)
                    {
                        if (!ShouldCheckWord(wordList, camelWord))
                        {
                            continue;
                        }

                        var location = Location.Create(tree, new TextSpan(baseOffset + camelOffset, camelWord.Length));
                        Report(context, camelWord, location);
                    }

                    continue;
                }

                var wordLocation = Location.Create(tree, new TextSpan(baseOffset + partOffset + wordOffset, word.Length));
                Report(context, word, wordLocation);
            }
        }
    }

    private static IEnumerable<(string Part, int Offset)> SplitByWhitespace(string text)
    {
        var currentPart = string.Empty;
        var partStart = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                if (currentPart.Length > 0)
                {
                    yield return (currentPart, partStart);
                    currentPart = string.Empty;
                }
                partStart = i + 1;
            }
            else
            {
                if (currentPart.Length == 0)
                {
                    partStart = i;
                }
                currentPart += text[i];
            }
        }

        if (currentPart.Length > 0)
        {
            yield return (currentPart, partStart);
        }
    }

    private static bool IsGuid(string text)
    {
        var guidRegex = new Regex(
            "^[({]?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}[)}]?$", RegexOptions.IgnoreCase);
        return guidRegex.IsMatch(text);
    }

    private static bool IsUrl(string text)
    {
        var urlRegex = new Regex("^[a-zA-Z][a-zA-Z0-9+.-]*://", RegexOptions.IgnoreCase);
        return urlRegex.IsMatch(text);
    }

    private static bool ShouldCheckWord(WordList wordList, string word)
    {
        if (word.Length <= 2)
        {
            return false;
        }

        return !wordList.Check(word);
    }

    private static void Report(SemanticModelAnalysisContext context, string word, Location location)
    {
        var diagnostic = Diagnostic.Create(
            rule,
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
            foreach (var part in StringHelper.SplitCamelCase(segment.Word, segment.Offset))
            {
                yield return part;
            }
        }
    }
}
