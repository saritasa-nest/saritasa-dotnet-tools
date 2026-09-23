using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;
using System.Collections.Immutable;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;

/// <summary>
/// Roslyn diagnostic analyzer that enforces navigation-property include rules (INCL001-INCL006).
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

        // INCL005: an [assembly: PassesIncludes] that names nothing.
        // INCL006: a [PassesIncludes] that is not allowed on the method it describes.
        context.RegisterSyntaxNodeAction(BridgeAttributeHandler.Analyze, SyntaxKind.Attribute);

        context.RegisterCompilationStartAction(compilationStart =>
        {
            // Reading [PassesIncludes] out of every referenced assembly is the expensive part, so it is
            // done once here and handed to every method body of the compilation.
            var bridges = Bridges.Read(compilationStart.Compilation);

            // INCL001 at call sites, INCL002, INCL003, INCL004: need the whole method body (control flow graph).
            compilationStart.RegisterOperationBlockAction(
                blockContext => IncludeFlowHandler.Analyze(blockContext, bridges));
        });
    }
}
