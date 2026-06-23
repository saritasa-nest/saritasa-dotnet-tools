using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Warns when an <c>else</c> branch returns after an <c>if</c> branch that already returns, suggesting early exit.
/// </summary>
/// <remarks>
/// Prefer early exits over nested <c>else</c> blocks after a return for simpler control flow.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EarlyExitAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "STAN1004";
    private const string Category = "Style";

    private static readonly LocalizableString Title = "Use early return instead of else after return";
    private static readonly LocalizableString MessageFormat = "Else branch returns after an if that already returns; prefer early exit";
    private static readonly LocalizableString Description = "When an if branch returns, the else can be flattened into an early return to simplify control flow.";

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
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IfStatementSyntax ifStatement)
        {
            return;
        }

        if (ifStatement.Else is null)
        {
            return;
        }

        if (!ExitsFromStatement(ifStatement.Statement))
        {
            return;
        }

        // If the else is an else-if, warn when that branch also exits.
        if (ifStatement.Else.Statement is IfStatementSyntax elseIf)
        {
            if (!ExitsFromStatement(elseIf.Statement))
            {
                return;
            }

            var elseIfDiagnostic = Diagnostic.Create(Rule, ifStatement.Else.ElseKeyword.GetLocation());
            context.ReportDiagnostic(elseIfDiagnostic);
            return;
        }

        if (!ExitsFromStatement(ifStatement.Else.Statement))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, ifStatement.Else.ElseKeyword.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool ExitsFromStatement(StatementSyntax statement)
    {
        switch (statement)
        {
            case ReturnStatementSyntax:
            case ThrowStatementSyntax:
                return true;
            case BlockSyntax block when block.Statements.Count == 1:
                return ExitsFromStatement(block.Statements[0]);
            case IfStatementSyntax nestedIf when ExitsFromStatement(nestedIf.Statement) && nestedIf.Else is not null && ExitsFromStatement(nestedIf.Else.Statement):
                return true;
            default:
                return false;
        }
    }
}
