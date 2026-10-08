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

    /// <summary>
    /// Diagnostic property name that contains the word that was found to have a typo.
    /// </summary>
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
            var wordList = SpellChecker.CreateWordList(compilationContext.Options, compilationContext.Compilation.SyntaxTrees);
            var namesRegex = SpellChecker.BuildNamesRegex(wordList);
            var compilation = compilationContext.Compilation;

            compilationContext.RegisterSemanticModelAction(c => AnalyzeSemanticModel(c, wordList, namesRegex, compilation));
        });
    }

    private static void AnalyzeSemanticModel(
        SemanticModelAnalysisContext context, WordList wordList, Regex? namesRegex, Compilation compilation)
    {
        var semanticModel = context.SemanticModel;
        var tree = semanticModel.SyntaxTree;
        var root = tree.GetRoot(context.CancellationToken);

        var descendantTokens = root.DescendantTokens(descendIntoTrivia: true);
        foreach (var token in descendantTokens)
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                CheckTrivia(context, tree, wordList, namesRegex, trivia);
            }

            foreach (var trivia in token.TrailingTrivia)
            {
                CheckTrivia(context, tree, wordList, namesRegex, trivia);
            }

            if (token.IsKind(SyntaxKind.IdentifierToken))
            {
                CheckIdentifierToken(context, tree, wordList, namesRegex, token, semanticModel, compilation);
            }

            if (IsString(token))
            {
                CheckText(context, tree, wordList, namesRegex, token.Text, token.Span.Start);
            }
        }
    }

    private static void CheckTrivia(
        SemanticModelAnalysisContext context,
        SyntaxTree tree,
        WordList wordList,
        Regex? namesRegex,
        SyntaxTrivia trivia)
    {
        if (!IsCommentTrivia(trivia))
        {
            return;
        }

        var text = trivia.ToFullString();
        CheckText(context, tree, wordList, namesRegex, text, trivia.FullSpan.Start);
    }

    private static void CheckIdentifierToken(
        SemanticModelAnalysisContext context,
        SyntaxTree tree,
        WordList wordList,
        Regex? namesRegex,
        SyntaxToken token,
        SemanticModel semanticModel,
        Compilation compilation)
    {
        if (IsExternalSymbol(token, semanticModel, compilation))
        {
            return;
        }

        var tokenText = token.ValueText;
        if (IsValidWord(wordList, tokenText))
        {
            return;
        }

        var maskedText = MaskNames(tokenText, namesRegex);
        var words = SplitIdentifier(maskedText);
        foreach (var (word, offset) in words)
        {
            if (IsValidWord(wordList, word))
            {
                continue;
            }

            Report(context, tree, word, token.Span.Start, offset);
        }
    }

    private static bool IsExternalSymbol(SyntaxToken token, SemanticModel semanticModel, Compilation compilation)
    {
        var node = token.Parent;
        if (node is null)
        {
            return false;
        }

        var symbol = semanticModel.GetSymbolInfo(node).Symbol ?? semanticModel.GetDeclaredSymbol(node);
        var containingAssembly = symbol?.ContainingAssembly;
        if (containingAssembly is null)
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
        Regex? namesRegex,
        string text,
        int baseOffset)
    {
        text = MaskIgnoredText(text);
        text = MaskNames(text, namesRegex);

        var words = StringHelper.SplitByNonLetters(text);
        foreach (var (word, offset) in words)
        {
            if (IsValidWord(wordList, word))
            {
                continue;
            }

            if (StringHelper.IsCamelCaseWord(word))
            {
                var camelCaseWords = StringHelper.SplitCamelCase(word, offset);
                foreach (var (camelWord, camelOffset) in camelCaseWords)
                {
                    if (IsValidWord(wordList, camelWord))
                    {
                        continue;
                    }

                    Report(context, tree, camelWord, baseOffset, camelOffset);
                }

                continue;
            }

            Report(context, tree, word, baseOffset, offset);
        }
    }

    /// <summary>
    /// Masks names in the given text by replacing them with spaces.
    /// This prevents tech names from being incorrectly split by camelCase splitting.
    /// </summary>
    /// <remarks>
    /// For example, "MediatR" will be split to "Mediat" and "R" and word "Mediat" will have warning.
    /// To prevent this we mask such names beforehand. So "MediatRModule" will become "       Module".
    /// </remarks>
    private static string MaskNames(string text, Regex? namesRegex)
    {
        if (namesRegex is null)
        {
            return text;
        }

        return namesRegex.Replace(
            text,
            match => IsCamelCaseMatch(text, match) ? MaskWithWhitespaces(match) : match.Value);
    }

    /// <summary>
    /// Checks that the match is a separate word in a camelCase identifier.
    /// </summary>
    /// <param name="text">Text where the match was found.</param>
    /// <param name="match">Name match found in the text.</param>
    /// <returns>
    /// <c>true</c> if the match is a separate word. For example, "NLog" in "NLogProvider", "myNLog" or "_nLog".
    /// <c>false</c> if the match is a part of a longer word, so it must be left as is.
    /// For example, "NLog" in "NLogs" or "nLog" in "DomainLogger".
    /// </returns>
    private static bool IsCamelCaseMatch(string text, Match match)
    {
        var endIndex = match.Index + match.Length;
        var continuesWithLowerCase = endIndex < text.Length && char.IsLower(text[endIndex]);
        if (continuesWithLowerCase)
        {
            return false;
        }

        var isTextStart = match.Index == 0;
        var startsWithUpperCase = char.IsUpper(text[match.Index]);
        var followsLowerCase = !isTextStart && char.IsLower(text[match.Index - 1]);

        return !followsLowerCase || startsWithUpperCase;
    }

    /// <summary>
    /// Masks text that should be ignored from analysis.
    /// </summary>
    /// <param name="text">Text where ignored text should be masked.</param>
    /// <returns>
    /// Text with ignored parts masked. For example, this sentence:
    /// <c>This GUID 1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d is ignored.</c>
    /// Will become:
    /// <c>This GUID                                      is ignored.</c>.
    /// </returns>
    /// <remarks>
    /// Note that we replace ignored letters with spaces,
    /// not just removing letters because we need to maintain text offset so we can report location correctly.
    /// </remarks>
    private static string MaskIgnoredText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = MaskUrl(text);
        text = MaskGuid(text);
        text = MaskHex(text);
        text = MaskFilePath(text);
        text = MaskFormatString(text);
        text = MaskAlphanumericToken(text);

        return text;
    }

    private static readonly Regex urlRegex = new(
        @"(https?://|www\.)[^\s\]\)]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Masks URLs, e.g. <c>https://example.com/some/path</c> or <c>www.example.com</c>.
    /// </summary>
    private static string MaskUrl(string text)
    {
        return urlRegex.Replace(text, ReplaceWithWhitespaces());
    }

    private static readonly Regex guidRegex = new(
        "[({]?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}[)}]?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <summary>
    /// Masks GUIDs, e.g. <c>1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d</c> or <c>{1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d}</c>.
    /// </summary>
    private static string MaskGuid(string text)
    {
        return guidRegex.Replace(text, ReplaceWithWhitespaces());
    }

    private static readonly Regex hexRegex = new(
        @"(?:0x[0-9a-fA-F]+|#[0-9a-fA-F]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Masks hex values, e.g. <c>0xFF00AA</c> or color codes like <c>#1a2b3c</c>.
    /// </summary>
    private static string MaskHex(string text)
    {
        return hexRegex.Replace(text, ReplaceWithWhitespaces());
    }

    private static readonly Regex filePathRegex = new(
        @"(?:^|(?<=\s))[a-zA-Z]:\\(?:[^\r\n\\]*\\)*[^\s\r\n]*|\\\\[^\s\r\n]+|(?:\.\./|\./)(?:[^\s\r\n]+)|/[a-zA-Z][^\s\r\n]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Masks file paths, e.g. <c>C:\Projects\app\file.txt</c>, <c>\\server\share</c>, <c>./config/app.json</c>
    /// or <c>/usr/local/bin</c>.
    /// </summary>
    private static string MaskFilePath(string text)
    {
        return filePathRegex.Replace(text, ReplaceWithWhitespaces());
    }

    private static readonly Regex formatStringRegex = new(
        @"\b[yMdHhmsfFtKz:/\-_\.]{3,}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Masks date and time format strings, e.g. <c>yyyy-MM-dd</c> or <c>HH:mm:ss</c>.
    /// </summary>
    private static string MaskFormatString(string text)
    {
        return formatStringRegex.Replace(text, ReplaceWithWhitespaces());
    }

    private static readonly Regex alphanumericTokenRegex = new(
        @"\b(?=[a-zA-Z0-9]*[a-zA-Z])(?=[a-zA-Z0-9]*[0-9])[a-zA-Z0-9]+\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Masks tokens that mix letters and digits, e.g. identifiers like <c>1234qwer</c> or <c>sha256</c>,
    /// or hashes like <c>43B3549CD700A288AFEA419C45C9CF2FA97DB3D4522652AE080280114EEAEF24</c>.
    /// </summary>
    private static string MaskAlphanumericToken(string text)
    {
        return alphanumericTokenRegex.Replace(text, ReplaceWithWhitespaces());
    }

    private static MatchEvaluator ReplaceWithWhitespaces() => MaskWithWhitespaces;

    private static string MaskWithWhitespaces(Match match) => new(' ', match.Length);

    private static bool IsValidWord(WordList wordList, string word)
    {
        if (word.Length <= 2)
        {
            return true;
        }

        if (wordList.Check(word))
        {
            return true;
        }

        // We also check with upper-case first letter in case the word stored in that way in dictionary.
        // For example, word "Monday" stored with upper-case first letter, but we could use it in identifier
        // where we have to use it with lower-case first letter.
        var titleCased = char.ToUpperInvariant(word[0]) + word.Substring(1);
        return wordList.Check(titleCased);
    }

    private static void Report(
        SemanticModelAnalysisContext context, SyntaxTree tree, string word, int baseOffset, int wordOffset)
    {
        var location = Location.Create(tree, new TextSpan(baseOffset + wordOffset, word.Length));
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
