using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;

internal static class InvocationHandler
{
    public static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        if (context.ContainingSymbol is not IMethodSymbol containingMethod)
        {
            return;
        }

        foreach (var attr in invocation.TargetMethod.GetAttributes())
        {
            if (!AttributeHelper.TryGetIncludeRequiredArgs(attr, out var targetParamName, out var propertyName))
            {
                continue;
            }

            // Resolve the exact parameter symbol in the called method that the attribute
            // references by name (e.g. "user" in [IncludeRequired("user", "Profile")]).
            var targetParam = invocation.TargetMethod.Parameters
                .FirstOrDefault(p => p.Name == targetParamName);
            if (targetParam is null)
            {
                continue;
            }

            // Match the resolved parameter symbol to the actual argument at this call site.
            var argument = invocation.Arguments
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.Parameter, targetParam));
            if (argument is null)
            {
                continue;
            }

            // --- INCL001: argument is one of the calling method's own parameters ----------
            if (argument.Value is IParameterReferenceOperation callerParamRef)
            {
                var callerParamName = callerParamRef.Parameter.Name;

                if (AttributeHelper.MethodHasIncludeRequiredAttribute(containingMethod, callerParamName, propertyName!))
                {
                    continue;
                }

                var typeName = callerParamRef.Parameter.Type.Name;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NavigationInclncludeRulesProvider.GetDiagnosticDescriptor(NavigationInclncludeRulesProvider.RuleIncl1Id),
                        invocation.Syntax.GetLocation(),
                        typeName,
                        propertyName,
                        callerParamName));
                continue;
            }

            // --- INCL002: argument is a local variable -----------------------------------
            if (argument.Value is ILocalReferenceOperation localRef)
            {
                // Check if the initializer has the include requirement:
                if (LocalValueHasInclude(localRef, propertyName!))
                {
                    continue;
                }

                var localTypeName = localRef.Local.Type.Name;
                var localVarName = localRef.Local.Name;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NavigationInclncludeRulesProvider.GetDiagnosticDescriptor(NavigationInclncludeRulesProvider.RuleIncl2Id),
                        invocation.Syntax.GetLocation(),
                        localTypeName,
                        propertyName,
                        localVarName));
            }
        }
    }

    private static bool LocalValueHasInclude(ILocalReferenceOperation localRef, string propertyName)
    {
        var initializer = NavigationPropertyLoadChecker.FindLocalInitializer(localRef);
        return initializer is not null && NavigationPropertyLoadChecker.InitializerHasInclude(initializer, propertyName);
    }
}
