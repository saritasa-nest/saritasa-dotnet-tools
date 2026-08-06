using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                => AppendDotToFormatString(invocation),
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
        var oldValueText = oldToken.ValueText;

        if (oldValueText.Length == 0)
        {
            return literal;
        }

        var newValueText = oldValueText + ".";
        var newText = oldToken.Text.Replace(oldValueText, newValueText);
        var newToken = SyntaxFactory.Token(
            oldToken.LeadingTrivia,
            oldToken.Kind(),
            newText,
            newValueText,
            oldToken.TrailingTrivia);
        return literal.WithToken(newToken);
    }

    private static InterpolatedStringExpressionSyntax AppendDotToInterpolatedString(
        InterpolatedStringExpressionSyntax interpolated)
    {
        var contents = interpolated.Contents;

        if (contents.Count > 0 && contents[contents.Count - 1] is InterpolatedStringTextSyntax lastText)
        {
            var oldToken = lastText.TextToken;
            var newToken = SyntaxFactory.Token(
                oldToken.LeadingTrivia,
                SyntaxKind.InterpolatedStringTextToken,
                oldToken.Text + ".",
                oldToken.ValueText + ".",
                oldToken.TrailingTrivia);
            return interpolated.WithContents(contents.Replace(lastText, lastText.WithTextToken(newToken)));
        }

        var dotText = SyntaxFactory.InterpolatedStringText(
            SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken, ".", ".", SyntaxTriviaList.Empty));
        return interpolated.WithContents(contents.Add(dotText));
    }

    private static InvocationExpressionSyntax AppendDotToFormatString(InvocationExpressionSyntax invocation)
    {
        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return invocation;
        }

        var firstArg = invocation.ArgumentList.Arguments[0];
        if (firstArg.Expression is LiteralExpressionSyntax literal)
        {
            return invocation.ReplaceNode(literal, AppendDotToStringLiteral(literal));
        }

        return invocation;
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
        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return false;
        }

        var firstArg = invocation.ArgumentList.Arguments[0];
        if (firstArg.Expression is not LiteralExpressionSyntax literal
            || !literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return false;
        }

        if (semanticModel is null)
        {
            return false;
        }

        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return false;
        }

        return method.IsStringFormat();
    }
}
