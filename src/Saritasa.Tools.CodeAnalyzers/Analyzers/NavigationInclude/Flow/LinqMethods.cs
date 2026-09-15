using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Facts about LINQ and EF Core methods needed by the analysis.
/// </summary>
internal static class LinqMethods
{
    private const string EfQueryableExtensions = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";

    private static readonly ImmutableHashSet<string> linqAndEfTypes = ImmutableHashSet.Create(
        "System.Linq.Enumerable",
        "System.Linq.Queryable",
        EfQueryableExtensions);

    /// <summary>
    /// Methods that return the same entities as their source (include, filter, sort, materialize).
    /// The "Async" suffix is ignored.
    /// </summary>
    private static readonly ImmutableHashSet<string> methodsKeepingSourceEntities = ImmutableHashSet.Create(
        "Include", "ThenInclude",
        "Where", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Skip", "Take", "Distinct", "Reverse",
        "AsEnumerable", "AsQueryable", "AsNoTracking", "AsNoTrackingWithIdentityResolution", "AsTracking",
        "AsSplitQuery", "AsSingleQuery",
        "First", "FirstOrDefault", "Single", "SingleOrDefault", "Last", "LastOrDefault",
        "ElementAt", "ElementAtOrDefault", "ToList", "ToArray", "ToHashSet", "ToDictionary");

    /// <summary>
    /// Methods that pass each element of the source to their lambda: <c>users.Select(u =&gt; ...)</c>.
    /// </summary>
    private static readonly ImmutableHashSet<string> methodsPassingElementsToLambda = ImmutableHashSet.Create(
        "Select", "SelectMany", "Where", "Any", "All", "Count", "First", "FirstOrDefault", "Single", "SingleOrDefault",
        "Last", "LastOrDefault", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "GroupBy",
        "ToDictionary", "Sum", "Min", "Max", "Average", "TakeWhile", "SkipWhile");

    /// <summary>
    /// Returns true if the method returns the same entities as its source, e.g. <c>Where</c> or <c>ToListAsync</c>.
    /// <c>ToDictionary(u =&gt; u.Id)</c> keeps the entities as values; <c>ToDictionary(u =&gt; u.Id, u =&gt; u.Name)</c>
    /// stores other objects.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <returns>True if the entities are kept.</returns>
    public static bool KeepsSourceEntities(IMethodSymbol method)
    {
        return IsLinqOrEfMethod(method) &&
               methodsKeepingSourceEntities.Contains(RemoveAsyncSuffix(method.Name)) &&
               !method.Parameters.Any(p => p.Name == "elementSelector");
    }

    /// <summary>
    /// Returns true for methods like <c>Select</c> or <c>Any</c> whose lambda receives elements of the source.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <returns>True if the lambda parameter is an element of the source.</returns>
    public static bool PassesElementsToLambda(IMethodSymbol method)
    {
        return IsLinqOrEfMethod(method) && methodsPassingElementsToLambda.Contains(RemoveAsyncSuffix(method.Name));
    }

    /// <summary>
    /// For <c>users.Select(u =&gt; ...)</c> returns <c>users</c>: the collection whose elements the lambda receives.
    /// Null if the lambda is not passed to such a LINQ method.
    /// </summary>
    /// <param name="lambda">Lambda.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? GetElementSource(IFlowAnonymousFunctionOperation lambda)
    {
        var parent = lambda.Parent;
        while (parent is IConversionOperation or IDelegateCreationOperation)
        {
            parent = parent.Parent;
        }

        if (parent is IArgumentOperation { Parent: IInvocationOperation call } && PassesElementsToLambda(call.TargetMethod))
        {
            return GetSource(call);
        }

        return null;
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
    /// Returns "Profile" for <c>Include(u =&gt; u.Profile)</c> or <c>Include("Profile")</c>;
    /// null for other calls. Filtered includes such as <c>u =&gt; u.Orders.Where(...)</c> are not recognized.
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Included property name or null.</returns>
    public static string? GetIncludedProperty(IInvocationOperation call)
    {
        var method = call.TargetMethod;
        if (method.Name != "Include" ||
            method.ContainingType.ToDisplayString() != EfQueryableExtensions ||
            call.Arguments.Length < 2)
        {
            return null;
        }

        var argument = call.Arguments[1].Value;
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
