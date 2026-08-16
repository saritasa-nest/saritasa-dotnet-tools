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
                // Check if the initializer satisfies the include requirement:
                if (LocalSatisfiesInclude(localRef, propertyName!))
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

    private static bool LocalSatisfiesInclude(ILocalReferenceOperation localRef, string propertyName)
    {
        var initializer = FindLocalInitializer(localRef);
        return initializer is not null && InitializerSatisfiesInclude(initializer, propertyName);
    }

    private static IOperation? FindLocalInitializer(ILocalReferenceOperation localRef)
    {
        var localSymbol = localRef.Local;

        // Walk up to the root of the operation tree.
        IOperation root = localRef;
        while (root.Parent is not null)
        {
            root = root.Parent;
        }

        return FindDeclaratorInitializer(root, localSymbol);
    }

    private static IOperation? FindDeclaratorInitializer(IOperation node, ILocalSymbol target)
    {
        if (node is IVariableDeclaratorOperation declarator &&
            SymbolEqualityComparer.Default.Equals(declarator.Symbol, target))
        {
            return declarator.Initializer?.Value;
        }

        foreach (var child in node.ChildOperations)
        {
            var result = FindDeclaratorInitializer(child, target);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static bool InitializerSatisfiesInclude(IOperation initializer, string propertyName)
    {
        // Object initializer: new T { Property = ... }
        if (initializer is IObjectCreationOperation objCreation)
        {
            return ObjectInitializerSetsProperty(objCreation, propertyName);
        }

        // Invocation chain (possibly async): check for .Include() or [Includes] on the method.
        return InvocationChainContainsInclude(initializer, propertyName);
    }

    private static bool InvocationChainContainsInclude(IOperation root, string propertyName)
    {
        var current = root;
        while (current is not null)
        {
            // Unwrap await.
            if (current is IAwaitOperation awaitOp)
            {
                current = awaitOp.Operation;
                continue;
            }

            if (current is not IInvocationOperation invocation)
            {
                break;
            }

            // Is this call itself an Include(x => x.Property)?
            if (IsIncludeCallForProperty(invocation, propertyName))
            {
                return true;
            }

            // Does the called method carry [Includes("Property")]?
            if (AttributeHelper.MethodHasIncludesAttribute(invocation.TargetMethod, propertyName))
            {
                return true;
            }

            // Advance to the receiver / next link in the chain.
            // For instance methods, Instance is the receiver.
            // For extension methods in non-reduced form, the receiver is Arguments[0].
            var next = invocation.Instance;
            if (next is null && invocation.Arguments.Length > 0)
            {
                next = invocation.Arguments[0].Value;
            }

            current = next;
        }

        return false;
    }

    private static bool IsIncludeCallForProperty(IInvocationOperation invocation, string propertyName)
    {
        if (!string.Equals(invocation.TargetMethod.Name, "Include", StringComparison.Ordinal))
        {
            return false;
        }

        foreach (var arg in invocation.Arguments)
        {
            if (LambdaAccessesProperty(arg.Value, propertyName))
            {
                return true;
            }
        }

        return false;
    }

    private static bool LambdaAccessesProperty(IOperation operation, string propertyName)
    {
        if (operation is IDelegateCreationOperation delegateCreation)
        {
            operation = delegateCreation.Target;
        }

        if (operation is IAnonymousFunctionOperation lambda)
        {
            return OperationContainsPropertyAccess(lambda.Body, propertyName);
        }

        return false;
    }

    private static bool OperationContainsPropertyAccess(IOperation node, string propertyName)
    {
        if (node is IPropertyReferenceOperation propRef &&
            string.Equals(propRef.Property.Name, propertyName, StringComparison.Ordinal))
        {
            return true;
        }

        foreach (var child in node.ChildOperations)
        {
            if (OperationContainsPropertyAccess(child, propertyName))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ObjectInitializerSetsProperty(IObjectCreationOperation objCreation, string propertyName)
    {
        if (objCreation.Initializer is null)
        {
            return false;
        }

        foreach (var init in objCreation.Initializer.Initializers)
        {
            if (init is ISimpleAssignmentOperation assignment &&
                assignment.Target is IPropertyReferenceOperation assignedProp &&
                string.Equals(assignedProp.Property.Name, propertyName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
