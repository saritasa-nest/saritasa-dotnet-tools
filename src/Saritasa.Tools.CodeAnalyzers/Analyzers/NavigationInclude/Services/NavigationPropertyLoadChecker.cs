using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

internal static class NavigationPropertyLoadChecker
{
    public static IOperation? FindLocalInitializer(ILocalReferenceOperation localRef)
    {
        var localSymbol = localRef.Local;

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

    public static bool InitializerHasInclude(IOperation initializer, string propertyName)
    {
        if (initializer is IObjectCreationOperation objCreation)
        {
            return ObjectInitializerSetsProperty(objCreation, propertyName);
        }

        return InvocationChainContainsInclude(initializer, propertyName);
    }

    private static bool InvocationChainContainsInclude(IOperation root, string propertyName)
    {
        var current = root;
        while (current is not null)
        {
            if (current is IAwaitOperation awaitOp)
            {
                current = awaitOp.Operation;
                continue;
            }

            if (current is not IInvocationOperation invocation)
            {
                break;
            }

            if (IsIncludeCallForProperty(invocation, propertyName))
            {
                return true;
            }

            if (AttributeHelper.MethodHasIncludesAttribute(invocation.TargetMethod, propertyName))
            {
                return true;
            }

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
