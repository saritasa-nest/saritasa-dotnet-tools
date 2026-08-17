using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// Provides helpers for detecting whether a navigation property is loaded for a given operation.
/// </summary>
internal static class NavigationPropertyLoadChecker
{
    /// <summary>
    /// Walks up the operation tree from local reference to find the initializer of the referenced local variable.
    /// </summary>
    /// <param name="localRef">A reference to the local variable whose initializer is sought.</param>
    /// <returns>The initializer value or null if the variable has no explicit initializer.</returns>
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

    /// <summary>
    /// Recursively searches node for the declarator of target and returns its initializer value.
    /// </summary>
    /// <param name="node">The root of the operation subtree to search.</param>
    /// <param name="target">The local symbol to look for.</param>
    /// <returns>The initializer value, or null if the declarator is not found in the subtree.</returns>
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

    /// <summary>
    /// Returns true if initializer loads the navigation property named property name.
    /// </summary>
    /// <param name="initializer">The operation that produces the value (object creation or query chain).</param>
    /// <param name="propertyName">The name of the navigation property to look for.</param>
    /// <returns>True if the property is loaded; otherwise false.</returns>
    public static bool InitializerHasInclude(IOperation initializer, string propertyName)
    {
        if (initializer is IObjectCreationOperation objCreation)
        {
            return ObjectInitializerSetsProperty(objCreation, propertyName);
        }

        return InvocationChainContainsInclude(initializer, propertyName);
    }

    /// <summary>
    /// Walks the method-call chain checking for a .Include() call or an <see cref="IncludesAttribute"/>-annotated method for property name.
    /// </summary>
    /// <param name="root">The operation in the invocation chain.</param>
    /// <param name="propertyName">The navigation property name to look for.</param>
    /// <returns>True if the chain loads the property; otherwise false.</returns>
    private static bool InvocationChainContainsInclude(IOperation root, string propertyName)
    {
        var current = root;
        while (current is not null)
        {
            // Unwrap async terminators such as ToListAsync() or FirstOrDefaultAsync().
            if (current is IAwaitOperation awaitOp)
            {
                current = awaitOp.Operation;
                continue;
            }

            // Unwrap implicit conversions in the receiver chain, e.g. IIncludableQueryable<T,P> → IQueryable<T>.
            if (current is IConversionOperation convOp && convOp.IsImplicit)
            {
                current = convOp.Operand;
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

    /// <summary>
    /// Returns true if invocation is a .Include() call whose lambda accesses property name.
    /// </summary>
    /// <param name="invocation">The invocation to inspect.</param>
    /// <param name="propertyName">The navigation property name to look for.</param>
    /// <returns>True if this is a matching Include call; otherwise false.</returns>
    private static bool IsIncludeCallForProperty(IInvocationOperation invocation, string propertyName)
    {
        if (!string.Equals(invocation.TargetMethod.Name, "Include", StringComparison.Ordinal))
        {
            return false;
        }

        // Only EF Core's Include counts, not arbitrary methods named Include.
        if (invocation.TargetMethod.ContainingType.ToDisplayString() !=
            "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions")
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

    /// <summary>
    /// Returns true if operation is a lambda or delegate that accesses property name.
    /// </summary>
    /// <param name="operation">The argument value to inspect (expected to be a lambda or delegate).</param>
    /// <param name="propertyName">The property name to look for.</param>
    /// <returns>True if the lambda accesses the property; otherwise false.</returns>
    private static bool LambdaAccessesProperty(IOperation operation, string propertyName)
    {
        // Unwrap any nesting of IDelegateCreationOperation / IConversionOperation to reach the anonymous function.
        bool unwrapped;
        do
        {
            unwrapped = false;
            if (operation is IDelegateCreationOperation delegateCreation)
            {
                operation = delegateCreation.Target;
                unwrapped = true;
            }
            else if (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
                unwrapped = true;
            }
        }
        while (unwrapped);

        if (operation is IAnonymousFunctionOperation lambda)
        {
            return OperationContainsPropertyAccess(lambda.Body, propertyName);
        }

        return false;
    }

    /// <summary>
    /// Recursively returns true if any operation in the subtree of node is a reference to property name.
    /// </summary>
    /// <param name="node">The root of the operation subtree to search.</param>
    /// <param name="propertyName">The property name to look for.</param>
    /// <returns>True if the property is referenced anywhere in the subtree; otherwise false.</returns>
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

    /// <summary>
    /// Returns true if the object initializer in object creation assigns property name.
    /// </summary>
    /// <param name="objCreation">The object creation operation to inspect.</param>
    /// <param name="propertyName">The navigation property name to look for.</param>
    /// <returns>True if the initializer sets the property; otherwise false.</returns>
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
