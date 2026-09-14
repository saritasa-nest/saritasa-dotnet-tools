using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers;
using Saritasa.Tools.CodeAnalyzers.Helpers;

namespace Saritasa.Tools.CodeAnalyzers.CodeFixProviders;

/// <summary>
/// Code fix for <see cref="ExceptionMessageDotAnalyzer"/> that appends a dot to the exception message.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExceptionMessageDotCodeFixProvider))]
[Shared]
public sealed class ExceptionMessageDotCodeFixProvider : CodeFixProvider
{
    private const string Title = "Append dot to exception message";
    private const string Dot = ".";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(ExceptionMessageDotAnalyzer.DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == ExceptionMessageDotAnalyzer.DiagnosticId);
        if (diagnostic is null)
        {
            return;
        }

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is null)
        {
            return;
        }

        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        if (node is ArgumentSyntax argument)
        {
            node = argument.Expression;
        }

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

        if (node is not ExpressionSyntax expression || AppendDot(expression, semanticModel) is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                Title,
                cancellationToken => AddDot(context.Document, expression, cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> AddDot(
        Document document,
        ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        var newExpression = AppendDot(expression, semanticModel);
        if (newExpression is null)
        {
            return document;
        }

        var newRoot = root.ReplaceNode(expression, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax? AppendDot(ExpressionSyntax expression, SemanticModel? semanticModel)
    {
        return expression switch
        {
            LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression)
                => AppendDotToStringLiteral(literal),
            InterpolatedStringExpressionSyntax interpolated
                => AppendDotToInterpolatedString(interpolated),
            InvocationExpressionSyntax invocation when IsStringFormatInvocation(invocation, semanticModel)
                => AppendDotToFormatString(invocation, semanticModel),
            ConditionalExpressionSyntax conditional
                => AppendDotToConditional(conditional, semanticModel),
            BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression)
                => AppendDotToCoalesce(binary, semanticModel),
            BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression)
                => AppendDotToBinaryAdd(binary, semanticModel),
            SwitchExpressionSyntax switchExpr
                => AppendDotToSwitchExpression(switchExpr, semanticModel),
            _ => null
        };
    }

    private static LiteralExpressionSyntax AppendDotToStringLiteral(LiteralExpressionSyntax literal)
    {
        var oldToken = literal.Token;
        if (oldToken.ValueText.Length == 0)
        {
            return literal;
        }

        var newToken = AppendDotToToken(oldToken, oldToken.Kind());
        return newToken is null ? literal : literal.WithToken(newToken.Value);
    }

    private static InterpolatedStringExpressionSyntax AppendDotToInterpolatedString(
        InterpolatedStringExpressionSyntax interpolated)
    {
        var contents = interpolated.Contents;

        if (contents.Count > 0 && contents[contents.Count - 1] is InterpolatedStringTextSyntax lastText)
        {
            var newToken = AppendDotToToken(lastText.TextToken, SyntaxKind.InterpolatedStringTextToken);
            return newToken is null
                ? interpolated
                : interpolated.WithContents(contents.Replace(lastText, lastText.WithTextToken(newToken.Value)));
        }

        var dotText = SyntaxFactory.InterpolatedStringText(
            SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken, Dot, Dot, SyntaxTriviaList.Empty));
        return interpolated.WithContents(contents.Add(dotText));
    }

    private static SyntaxToken? AppendDotToToken(SyntaxToken oldToken, SyntaxKind tokenKind)
    {
        if (oldToken.ValueText.TrimEnd().EndsWith(Dot, StringComparison.Ordinal))
        {
            return null;
        }

        var newValueText = InsertDotBeforeTrailingWhitespace(oldToken.ValueText);

        var rawText = oldToken.Text;
        var (prefixLength, suffixLength) = GetDelimiterLengths(rawText, tokenKind);
        var prefix = rawText.Substring(0, prefixLength);
        var suffix = rawText.Substring(rawText.Length - suffixLength);
        var content = rawText.Substring(prefixLength, rawText.Length - prefixLength - suffixLength);
        var newText = prefix + InsertDotBeforeTrailingWhitespace(content) + suffix;

        return SyntaxFactory.Token(
            oldToken.LeadingTrivia,
            tokenKind,
            newText,
            newValueText,
            oldToken.TrailingTrivia);
    }

    private static string InsertDotBeforeTrailingWhitespace(string text)
    {
        var trimmed = text.TrimEnd();
        var trailingWhitespace = text.Substring(trimmed.Length);
        return trimmed + Dot + trailingWhitespace;
    }

    private static (int PrefixLength, int SuffixLength) GetDelimiterLengths(string rawText, SyntaxKind tokenKind)
    {
        if (tokenKind == SyntaxKind.InterpolatedStringTextToken)
        {
            return (0, 0);
        }

        const string rawStringPrefix = "\"\"\"";
        if (rawText.StartsWith(rawStringPrefix, StringComparison.Ordinal))
        {
            var length = 0;
            while (length < rawText.Length && rawText[length] == '"')
            {
                length++;
            }

            return (length, length);
        }

        const string verbatimStringPrefix = "@\"";
        if (rawText.StartsWith(verbatimStringPrefix, StringComparison.Ordinal))
        {
            return (2, 1);
        }

        return (1, 1);
    }

    private static InvocationExpressionSyntax AppendDotToFormatString(
        InvocationExpressionSyntax invocation,
        SemanticModel? semanticModel)
    {
        if (GetFormatArgument(invocation, semanticModel)?.Expression is LiteralExpressionSyntax literal)
        {
            return invocation.ReplaceNode(literal, AppendDotToStringLiteral(literal));
        }

        return invocation;
    }

    private static ArgumentSyntax? GetFormatArgument(InvocationExpressionSyntax invocation, SemanticModel? semanticModel)
    {
        if (semanticModel?.GetOperation(invocation) is not IInvocationOperation operation)
        {
            return null;
        }

        var formatArgument = operation.Arguments.FirstOrDefault(a => a.Parameter?.Name == "format");
        return formatArgument?.Syntax as ArgumentSyntax;
    }

    private static ExpressionSyntax? AppendDotToConditional(ConditionalExpressionSyntax conditional, SemanticModel? semanticModel)
    {
        var trueExpr = AppendDot(conditional.WhenTrue, semanticModel);
        var falseExpr = AppendDot(conditional.WhenFalse, semanticModel);
        if (trueExpr is null || falseExpr is null)
        {
            return null;
        }

        return conditional.WithWhenTrue(trueExpr).WithWhenFalse(falseExpr);
    }

    private static ExpressionSyntax? AppendDotToCoalesce(BinaryExpressionSyntax binary, SemanticModel? semanticModel)
    {
        var fixedRight = AppendDot(binary.Right, semanticModel);
        if (fixedRight is null)
        {
            return null;
        }

        return binary.WithRight(fixedRight);
    }

    private static ExpressionSyntax? AppendDotToBinaryAdd(BinaryExpressionSyntax binary, SemanticModel? semanticModel)
    {
        var rightmost = GetRightmostOperand(binary);
        var fixedRight = AppendDot(rightmost, semanticModel);
        if (fixedRight is null)
        {
            return null;
        }

        return binary.ReplaceNode(rightmost, fixedRight);
    }

    private static ExpressionSyntax? AppendDotToSwitchExpression(SwitchExpressionSyntax switchExpr, SemanticModel? semanticModel)
    {
        var newArms = new List<SwitchExpressionArmSyntax>();
        foreach (var arm in switchExpr.Arms)
        {
            var fixedValue = AppendDot(arm.Expression, semanticModel);
            if (fixedValue is null)
            {
                return null;
            }

            newArms.Add(arm.WithExpression(fixedValue));
        }

        return switchExpr.WithArms(SyntaxFactory.SeparatedList(newArms));
    }

    private static ExpressionSyntax GetRightmostOperand(BinaryExpressionSyntax binary)
    {
        var current = binary;
        while (current.Right is BinaryExpressionSyntax rightBinary && rightBinary.IsKind(SyntaxKind.AddExpression))
        {
            current = rightBinary;
        }

        return current.Right;
    }

    private static bool IsStringFormatInvocation(InvocationExpressionSyntax invocation, SemanticModel? semanticModel)
    {
        if (semanticModel?.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method || !method.IsStringFormat())
        {
            return false;
        }

        return GetFormatArgument(invocation, semanticModel)?.Expression is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression);
    }
}
