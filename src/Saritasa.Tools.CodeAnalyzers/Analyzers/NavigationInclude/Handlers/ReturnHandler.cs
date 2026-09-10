using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;

/// <summary>
/// Reports INCL003 when a <see cref="IncludesAttribute"/>-annotated method's return value
/// does not load the promised navigation property.
/// </summary>
internal static class ReturnHandler
{
    /// <summary>
    /// Analyzes a return operation and reports INCL003 if applicable.
    /// </summary>
    /// <param name="context">Operation analysis context.</param>
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
                        NavigationIncludeRulesProvider.GetDiagnosticDescriptor(NavigationIncludeRulesProvider.RuleIncl3Id),
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
