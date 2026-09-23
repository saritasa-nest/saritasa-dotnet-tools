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
/// One method per shape the search can be standing on, all landing through <see cref="GetCallSource"/>.
/// There is normally one value on the other side; a member holding the entities of two places gives two.
/// Nothing is inferred: what nobody declared is not crossed.
/// </remarks>
internal static class BridgeCrosser
{
    /// <summary>
    /// The values the entities came from. Empty when no bridge runs out of this value.
    /// </summary>
    /// <param name="value">Value the search is reading.</param>
    /// <returns>Sources.</returns>
    public static IReadOnlyList<IOperation> FromValue(Value value)
        => value.Operation switch
        {
            // "query.Where(...)", "users.ToList()", "query.Paginate(1)". Returns query/users
            IInvocationOperation call
                => GetCallSources(value.Position.FlowGraph.Bridges.FindFromResult(call.TargetMethod), call),

            // "users[0]", "enumerator.Current", "pair.Value", "page.Items": the entities come from the object
            // the property is read on. Returns users/enumerator/pair/page
            IPropertyReferenceOperation reference
                when value.Position.FlowGraph.Bridges.FindFromResult(reference.Property)
                    .Any(to => to is BridgeEnd.Instance)
                => Single(reference.Instance),

            // "users[0]" of an array. Roslyn exposes nothing to name here, so no declaration can describe
            // it, and there is only one value the element can come from. Returns users
            IArrayElementReferenceOperation element
                => Single(element.ArrayReference),

            _ => [],
        };

    /// <summary>
    /// The collection a lambda parameter is filled from: for the <c>u</c> of <c>users.Select(u =&gt; ...)</c>,
    /// the value <c>users</c>. Empty for a parameter nobody declared, such as the index of
    /// <c>Select((u, i) =&gt; ...)</c>.
    /// </summary>
    /// <param name="lambda">Lambda the parameter belongs to.</param>
    /// <param name="parameterOrdinal">Position of the parameter in the lambda.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    /// <returns>Sources.</returns>
    public static IReadOnlyList<IOperation> FromLambdaParameter(
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
            return [];
        }

        // The bridge names the callback and the position inside it, so no overload check is needed.
        return GetCallSources(
            bridges.FindFromParameter(call.TargetMethod, lambdaArgument.Name, parameterOrdinal),
            call);
    }

    /// <summary>
    /// The value an out argument came from: for <c>d.TryGetValue(k, out var user)</c>, the value <c>d</c>.
    /// </summary>
    /// <param name="call">Call that wrote the out argument.</param>
    /// <param name="outParameter">Name of the out parameter the entities arrived at.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    /// <returns>Sources.</returns>
    public static IReadOnlyList<IOperation> FromOutArgument(
        IInvocationOperation call,
        string outParameter,
        Bridges bridges)
        => GetCallSources(bridges.FindFromParameter(call.TargetMethod, outParameter), call);

    /// <summary>
    /// The values at the ends the bridges land on, leaving out any that is not there to read.
    /// </summary>
    private static IReadOnlyList<IOperation> GetCallSources(
        IReadOnlyList<BridgeEnd> ends,
        IInvocationOperation call)
        => ends.Count == 0
            ? []
            : ends.Select(to => GetCallSource(to, call)).OfType<IOperation>().ToList();

    /// <summary>
    /// The one value there is, or nothing when there is none.
    /// </summary>
    private static IReadOnlyList<IOperation> Single(IOperation? source)
        => source is null ? [] : [source];

    /// <summary>
    /// The value at the end a bridge lands on: what the method was called on, or the argument for a named
    /// parameter. A bridge never lands on a result.
    /// </summary>
    private static IOperation? GetCallSource(BridgeEnd to, IInvocationOperation call)
        => to switch
        {
            BridgeEnd.Instance => RoslynReader.GetReceiver(call),
            BridgeEnd.Parameter parameter
                => call.Arguments
                    .FirstOrDefault(argument => argument.Parameter?.Name == parameter.Name)
                    ?.Value,
            _ => null,
        };
}
