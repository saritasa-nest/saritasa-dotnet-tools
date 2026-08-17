using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;

/// <summary>
/// Roslyn diagnostic analyzer that enforces navigation-property include rules (INCL001–INCL003).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NavigationIncludeAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => NavigationIncludeRulesProvider.SupportedDiagnostics;

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(PropertyReferenceHandler.Analyze, OperationKind.PropertyReference);
        context.RegisterOperationAction(InvocationHandler.Analyze, OperationKind.Invocation);
        context.RegisterOperationAction(ReturnHandler.Analyze, OperationKind.Return);
    }
}
