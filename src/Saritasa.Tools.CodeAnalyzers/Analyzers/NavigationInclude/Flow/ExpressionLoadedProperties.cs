using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Answers "which navigation properties are loaded in the result of this expression?".
/// Only reads the <see cref="LoadedProperties"/> table, never changes it.
/// </summary>
internal static class ExpressionLoadedProperties
{
    private const string EfQueryableExtensions = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";

    private static readonly ImmutableHashSet<string> linqAndEfTypes = ImmutableHashSet.Create(
        "System.Linq.Enumerable",
        "System.Linq.Queryable",
        EfQueryableExtensions);

    /// <summary>
    /// Methods that return the same entities as their source (filter, sort, materialize),
    /// so the loaded properties of the source are kept. The "Async" suffix is ignored.
    /// </summary>
    private static readonly ImmutableHashSet<string> methodsKeepingSourceEntities = ImmutableHashSet.Create(
        "Where", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Skip", "Take", "Distinct", "Reverse",
        "AsEnumerable", "AsQueryable", "AsNoTracking", "AsNoTrackingWithIdentityResolution", "AsTracking",
        "AsSplitQuery", "AsSingleQuery",
        "First", "FirstOrDefault", "Single", "SingleOrDefault", "Last", "LastOrDefault",
        "ElementAt", "ElementAtOrDefault", "ToList", "ToArray", "ToHashSet");

    /// <summary>
    /// Methods that pass each element of the source to their lambda: <c>users.Select(u =&gt; ...)</c>.
    /// </summary>
    private static readonly ImmutableHashSet<string> methodsPassingElementsToLambda = ImmutableHashSet.Create(
        "Select", "SelectMany", "Where", "Any", "All", "Count", "First", "FirstOrDefault", "Single", "SingleOrDefault",
        "Last", "LastOrDefault", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "GroupBy",
        "ToDictionary", "Sum", "Min", "Max", "Average", "TakeWhile", "SkipWhile");

    /// <summary>
    /// Returns the navigation properties loaded in the result of the expression.
    /// </summary>
    /// <param name="expression">Expression.</param>
    /// <param name="table">Loaded properties of variables before the expression runs.</param>
    /// <returns>Loaded navigation property names.</returns>
    public static ImmutableHashSet<string> Get(IOperation? expression, LoadedProperties table)
    {
        expression = SkipAwaitAndConversions(expression);

        if (TryGetVariable(expression, out var variable))
        {
            return table.GetProperties(variable);
        }

        // foreach is rewritten by the compiler to "item = enumerator.Current".
        if (expression is IPropertyReferenceOperation { Property.Name: "Current" } current)
        {
            return Get(current.Instance, table);
        }

        if (expression is IInvocationOperation call)
        {
            return GetForMethodCall(call, table);
        }

        // dbContext.Users, null, fields, constants: nothing is known to be loaded.
        return LoadedProperties.NoProperties;
    }

    /// <summary>
    /// Returns the variable (local, parameter or compiler temporary) the operation reads.
    /// </summary>
    /// <param name="operation">Operation.</param>
    /// <param name="variable">Variable key for <see cref="LoadedProperties"/>.</param>
    /// <returns>True if the operation reads a variable.</returns>
    public static bool TryGetVariable(IOperation? operation, out object variable)
    {
        operation = SkipConversions(operation);

        if (operation is ILocalReferenceOperation local)
        {
            variable = local.Local;
            return true;
        }

        if (operation is IParameterReferenceOperation parameter)
        {
            variable = parameter.Parameter;
            return true;
        }

        if (operation is IFlowCaptureReferenceOperation capture)
        {
            variable = capture.Id;
            return true;
        }

        variable = null!;
        return false;
    }

    /// <summary>
    /// Removes casts and delegate wrappers around an operation.
    /// </summary>
    /// <param name="operation">Operation.</param>
    /// <returns>Operation without wrappers.</returns>
    public static IOperation? SkipConversions(IOperation? operation)
    {
        while (true)
        {
            if (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
                continue;
            }

            if (operation is IDelegateCreationOperation delegateCreation)
            {
                operation = delegateCreation.Target;
                continue;
            }

            return operation;
        }
    }

    /// <summary>
    /// Returns the collection or query the method is called on: <c>Instance</c> for instance methods,
    /// the first argument for extension methods (<c>query.Where(...)</c> is <c>Queryable.Where(query, ...)</c>).
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? GetSource(IInvocationOperation call)
    {
        if (call.Instance is not null)
        {
            return call.Instance;
        }

        if (call.TargetMethod.IsExtensionMethod && call.Arguments.Length > 0)
        {
            return call.Arguments[0].Value;
        }

        return null;
    }

    /// <summary>
    /// Returns true for LINQ methods like Select/Where/Any whose lambda receives elements of the source.
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>True if the lambda parameter is an element of the source.</returns>
    public static bool PassesElementsToLambda(IInvocationOperation call)
    {
        return IsLinqOrEfMethod(call.TargetMethod) &&
               methodsPassingElementsToLambda.Contains(RemoveAsyncSuffix(call.TargetMethod.Name));
    }

    private static ImmutableHashSet<string> GetForMethodCall(IInvocationOperation call, LoadedProperties table)
    {
        var method = call.TargetMethod;
        var promisedProperties = GetPropertiesPromisedByIncludesAttributes(method);

        // foreach is rewritten by the compiler to "enumerator = collection.GetEnumerator()".
        if (method.Name is "GetEnumerator" or "GetAsyncEnumerator")
        {
            return promisedProperties.Union(Get(call.Instance, table));
        }

        if (!IsLinqOrEfMethod(method))
        {
            return promisedProperties;
        }

        var sourceProperties = Get(GetSource(call), table);

        if (method.Name == "Include" && method.ContainingType.ToDisplayString() == EfQueryableExtensions)
        {
            var includedProperty = GetIncludedPropertyName(call);
            return includedProperty is null ? sourceProperties : sourceProperties.Add(includedProperty);
        }

        if (methodsKeepingSourceEntities.Contains(RemoveAsyncSuffix(method.Name)))
        {
            return sourceProperties.Union(promisedProperties);
        }

        // Select and other projections create new objects: their properties are not loaded.
        return promisedProperties;
    }

    private static ImmutableHashSet<string> GetPropertiesPromisedByIncludesAttributes(IMethodSymbol method)
    {
        var properties = LoadedProperties.NoProperties;
        foreach (var attribute in method.GetAttributes())
        {
            if (AttributeHelper.TryGetIncludesArg(attribute, out var property))
            {
                properties = properties.Add(property!);
            }
        }

        return properties;
    }

    /// <summary>
    /// Extracts "Profile" from <c>Include(u =&gt; u.Profile)</c> or <c>Include("Profile")</c>.
    /// Filtered includes such as <c>u =&gt; u.Orders.Where(...)</c> are not recognized.
    /// </summary>
    private static string? GetIncludedPropertyName(IInvocationOperation includeCall)
    {
        if (includeCall.Arguments.Length < 2)
        {
            return null;
        }

        var argument = includeCall.Arguments[1].Value;
        if (argument.ConstantValue is { HasValue: true, Value: string path })
        {
            return path;
        }

        // In the control flow graph the lambda body is a separate graph,
        // so the property name is read from the lambda syntax "u => u.Profile".
        if (argument.Syntax is LambdaExpressionSyntax { ExpressionBody: MemberAccessExpressionSyntax memberAccess })
        {
            return memberAccess.Name.Identifier.ValueText;
        }

        return null;
    }

    private static IOperation? SkipAwaitAndConversions(IOperation? operation)
    {
        while (true)
        {
            if (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
                continue;
            }

            if (operation is IAwaitOperation awaitOperation)
            {
                operation = awaitOperation.Operation;
                continue;
            }

            return operation;
        }
    }

    private static bool IsLinqOrEfMethod(IMethodSymbol method)
    {
        return linqAndEfTypes.Contains(method.ContainingType.ToDisplayString());
    }

    private static string RemoveAsyncSuffix(string methodName)
    {
        const string asyncSuffix = "Async";
        return methodName.EndsWith(asyncSuffix, StringComparison.Ordinal)
            ? methodName.Substring(0, methodName.Length - asyncSuffix.Length)
            : methodName;
    }
}
