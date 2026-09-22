using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Roslyn;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// Crosses a <see cref="Bridge"/>: from the value the search is standing on to the value the entities came
/// from — <c>query.Where(...)</c>, <c>users.ToList()</c>, <c>users[0]</c>, <c>page.Items</c>.
/// </summary>
/// <remarks>
/// The shape of the move does not matter, only that the entities on both sides are the same ones. One method
/// here per bridge kind, all ending in <see cref="GetCallSource"/>.
/// Nothing is inferred from a signature or a type: what nobody declared is not crossed.
/// </remarks>
internal static class BridgeCrosser
{
    /// <summary>
    /// The value the entities came from, or null when the move out of this value is not declared or there is
    /// nothing on the other side of it.
    /// </summary>
    /// <param name="value">Value the search is reading.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FromValue(Value value)
        => value.Operation switch
        {
            // "query.Where(...)", "users.ToList()", "query.Paginate(1)". Returns query/users
            IInvocationOperation call
                when value.Position.FlowGraph.Bridges.FindFromResult(call.TargetMethod) is { } from
                => GetCallSource(call, from),

            // "users[0]", "enumerator.Current", "pair.Value", "page.Items": the entities come from the object
            // the property is read on. Returns users/enumerator/pair/page
            IPropertyReferenceOperation reference
                when value.Position.FlowGraph.Bridges.FindFromResult(reference.Property) is not null
                => reference.Instance,

            // "users[0]" of an array. Roslyn exposes nothing to name here, so no declaration can describe
            // it, and there is only one value the element can come from. Returns users
            IArrayElementReferenceOperation element
                => element.ArrayReference,

            _ => null,
        };

    /// <summary>
    /// The collection a lambda parameter is filled from: for the <c>u</c> of <c>users.Select(u =&gt; ...)</c>,
    /// the value <c>users</c>. Null when nobody declared that this parameter is handed an element, as for the
    /// index of <c>Select((u, i) =&gt; ...)</c> or the key of <c>GroupBy</c>, or when the lambda is not passed
    /// to a call at all.
    /// </summary>
    /// <param name="lambda">Lambda the parameter belongs to.</param>
    /// <param name="parameterOrdinal">Position of the parameter in the lambda.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FromLambdaParameter(
        IFlowAnonymousFunctionOperation lambda,
        int parameterOrdinal,
        Bridges bridges)
    {
        var parent = lambda.Parent;
        while (parent is IConversionOperation or IDelegateCreationOperation)
        {
            parent = parent.Parent;
        }

        if (parent is not IArgumentOperation { Parameter: { } lambdaArgument, Parent: IInvocationOperation call })
        {
            return null;
        }

        // The bridge names the callback and the position inside it, so no overload check is needed.
        return bridges.FindFromLambdaParameter(
                call.TargetMethod,
                lambdaArgument.Name,
                parameterOrdinal) is { } from
            ? GetCallSource(call, from)
            : null;
    }

    /// <summary>
    /// The value an out argument's entities came from: for <c>d.TryGetValue(k, out var user)</c>, the
    /// value <c>d</c>. Null when nobody declared that the method writes entities there.
    /// </summary>
    /// <param name="call">Call that wrote the out argument.</param>
    /// <param name="outParameter">Name of the out parameter the entities arrived at.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FromOutArgument(
        IInvocationOperation call,
        string outParameter,
        Bridges bridges)
        => bridges.FindFromOutArgument(call.TargetMethod, outParameter) is { } from
            ? GetCallSource(call, from)
            : null;

    /// <summary>
    /// The value a call hands its entities back from.
    /// </summary>
    /// <remarks>
    /// Landing on no parameter means the value the method was called on; naming one means the argument
    /// passed for it.
    /// </remarks>
    private static IOperation? GetCallSource(IInvocationOperation call, string parameterName)
        => parameterName.Length == 0
            ? RoslynReader.GetReceiver(call)
            : call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == parameterName)?.Value;
}
