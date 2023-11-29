using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers
{
    /// <summary>
    /// Analyzer for request handlers.
    /// This code analyzer allows to diagnose missing return type in request handlers.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RequestHandlersAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Usage";

        /// <summary>
        /// Diagnostic id for request handlers analyzer.
        /// </summary>
        public const string DiagnosticId = "SaritasaToolsRequestHandlersAnalyzer";

        private static readonly LocalizableString title = "Request handler does not have a return type";
        private static readonly LocalizableString messageFormat = "Request handler '{0}' does not have a return type";
        private static readonly LocalizableString description = "Request handler must have a return type. " +
                                                                "Otherwise pipeline behavior won't work.";

        private static readonly DiagnosticDescriptor rule = new(
            DiagnosticId,
            title,
            messageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: description);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var requestHandlerInterface = namedTypeSymbol.Interfaces
                .FirstOrDefault(IncorrectRequestHandlerPredicate);

            if (requestHandlerInterface == null)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IncorrectRequestHandlerPredicate(INamedTypeSymbol namedTypeSymbol)
        {
            const int requestHandlerCountParameters = 2;
            if (!namedTypeSymbol.Name.StartsWith("IRequestHandler"))
            {
                return false;
            }

            return namedTypeSymbol.TypeParameters.Length != requestHandlerCountParameters;
        }
    }
}
