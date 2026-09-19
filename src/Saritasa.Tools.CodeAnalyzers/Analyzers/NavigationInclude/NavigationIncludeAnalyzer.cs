using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;

/// <summary>
/// Roslyn diagnostic analyzer that enforces navigation-property include rules (INCL001-INCL005).
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

        // INCL001 for direct access (user.Profile): one operation is enough.
        context.RegisterOperationAction(PropertyReferenceHandler.Analyze, OperationKind.PropertyReference);

        // INCL005: an [assembly: PreservesIncludes] that names nothing.
        context.RegisterSyntaxNodeAction(DeclarationHandler.Analyze, SyntaxKind.Attribute);

        context.RegisterCompilationStartAction(compilationStart =>
        {
            // Reading [PreservesIncludes] out of every referenced assembly is the expensive part, so it is
            // done once here and handed to every method body of the compilation.
            var declarations = IncludeDeclarations.Read(compilationStart.Compilation);

            // INCL001 at call sites, INCL002, INCL003, INCL004: need the whole method body (control flow graph).
            compilationStart.RegisterOperationBlockAction(
                blockContext => IncludeFlowHandler.Analyze(blockContext, declarations));
        });
    }
}
