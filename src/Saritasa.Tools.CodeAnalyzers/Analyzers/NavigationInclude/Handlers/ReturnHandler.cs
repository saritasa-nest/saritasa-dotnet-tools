using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;

internal static class ReturnHandler
{
    public static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IReturnOperation returnOp)
        {
            return;
        }

        var returnedValue = returnOp.ReturnedValue;
        if (returnedValue is null)
        {
            return;
        }

        if (context.ContainingSymbol is not IMethodSymbol containingMethod)
        {
            return;
        }

        // Skip returns that belong to a nested lambda or local function — we only want
        // top-level returns of the containing method itself.
        if (!IsDirectMethodReturn(returnOp))
        {
            return;
        }

        foreach (var attr in containingMethod.GetAttributes())
        {
            if (!AttributeHelper.TryGetIncludesArg(attr, out var propertyName))
            {
                continue;
            }

            if (!ReturnValueHasInclude(returnedValue, propertyName!))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NavigationInclncludeRulesProvider.GetDiagnosticDescriptor(NavigationInclncludeRulesProvider.RuleIncl3Id),
                        returnOp.Syntax.GetLocation(),
                        propertyName));
            }
        }
    }

    private static bool IsDirectMethodReturn(IReturnOperation returnOp)
    {
        var parent = returnOp.Parent;
        while (parent is not null)
        {
            if (parent is IAnonymousFunctionOperation or ILocalFunctionOperation)
            {
                return false;
            }

            parent = parent.Parent;
        }

        return true;
    }

    private static bool ReturnValueHasInclude(IOperation value, string propertyName)
    {
        if (value is ILocalReferenceOperation localRef)
        {
            var initializer = NavigationPropertyLoadChecker.FindLocalInitializer(localRef);
            return initializer is not null && NavigationPropertyLoadChecker.InitializerHasInclude(initializer, propertyName);
        }

        return NavigationPropertyLoadChecker.InitializerHasInclude(value, propertyName);
    }
}
