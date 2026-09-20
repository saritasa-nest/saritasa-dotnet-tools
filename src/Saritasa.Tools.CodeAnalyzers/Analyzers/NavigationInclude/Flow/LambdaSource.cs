using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Where a lambda parameter is filled from: for the <c>u</c> of <c>users.Select(u =&gt; ...)</c>,
/// the collection <c>users</c>.
/// </summary>
/// <remarks>
/// This is not a move between values, see <see cref="Transformation"/>, and no declaration can describe it:
/// a declaration says where a result comes from, while this question is about what goes in. The answer is read
/// from the method's own declaration, one parameter at a time.
/// </remarks>
internal static class LambdaSource
{
    /// <summary>
    /// The collection the lambda parameter is filled from. Null when that parameter holds something else, such
    /// as the index of <c>Select((u, i) =&gt; ...)</c> or the key of <c>GroupBy</c>, or when the method is not
    /// shaped like a LINQ operator at all.
    /// </summary>
    /// <param name="lambda">Lambda.</param>
    /// <param name="parameterOrdinal">Position of the lambda parameter in question.</param>
    /// <returns>Collection or null.</returns>
    public static IOperation? FindCollection(IFlowAnonymousFunctionOperation lambda, int parameterOrdinal)
    {
        var parent = lambda.Parent;
        while (parent is IConversionOperation or IDelegateCreationOperation)
        {
            parent = parent.Parent;
        }

        if (parent is IArgumentOperation { Parameter: { } lambdaArgument, Parent: IInvocationOperation call } &&
            ComesFromCollection(lambdaArgument, call.TargetMethod, parameterOrdinal))
        {
            return RoslynHelper.GetReceiver(call);
        }

        return null;
    }

    /// <summary>
    /// True if the lambda parameter at this position is filled from the collection the method is called on.
    /// </summary>
    /// <remarks>
    /// <c>Select</c> declares its selector <c>Func&lt;TSource, TResult&gt;</c>, so the parameter is an element;
    /// <c>Select((u, i) =&gt; ...)</c> declares <c>Func&lt;TSource, int, TResult&gt;</c>, so the second one is
    /// not. The result selector of <c>GroupBy</c> is <c>Func&lt;TKey, IEnumerable&lt;TSource&gt;, TResult&gt;</c>:
    /// the key is not filled from the collection, the group is. A project's own
    /// <c>ForEachItem(this IEnumerable&lt;T&gt;, Action&lt;T&gt;)</c> reads the same way.
    /// </remarks>
    private static bool ComesFromCollection(IParameterSymbol lambdaArgument, IMethodSymbol method, int ordinal)
    {
        var definition = method.OriginalDefinition;
        if (definition.Parameters.Length <= lambdaArgument.Ordinal ||
            RoslynHelper.GetReceiverType(definition) is not { } collection)
        {
            return false;
        }

        var declaredType = RoslynHelper.GetLambdaParameterType(
            definition.Parameters[lambdaArgument.Ordinal].Type,
            ordinal);

        return declaredType is not null &&
               RoslynHelper.GetTypesInside(collection, withInterfaces: true).Contains(declaredType);
    }
}
