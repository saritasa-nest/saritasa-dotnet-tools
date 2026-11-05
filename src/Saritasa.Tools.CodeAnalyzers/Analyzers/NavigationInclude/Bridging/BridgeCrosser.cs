using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Roslyn;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// Crosses a <see cref="Bridge"/>: from the value the search is standing on to the value the entities came
/// from — <c>query.Where(...)</c>, <c>users.ToList()</c>, <c>users[0]</c>, <c>pair.Value</c>,
/// <c>users.Select(u =&gt; u.Settings)</c>.
/// </summary>
/// <remarks>
/// One method per shape the search can be standing on, all landing through <see cref="GetCallCrossing"/>.
/// There is normally one value on the other side; a member holding the entities of two places gives two, and
/// the path must be loaded in both.
/// Nothing is inferred: what nobody annotated is not crossed.
/// </remarks>
internal static class BridgeCrosser
{
    /// <summary>
    /// The values the entities came from. Empty when no bridge runs out of this value.
    /// </summary>
    /// <param name="value">Value the search is reading.</param>
    /// <returns>Crossings.</returns>
    public static IReadOnlyList<Crossing> FromValue(Value value)
        => value.Operation switch
        {
            // "query.Where(...)", "users.ToList()", "query.Paginate(1)", "users.Select(u => u.Settings)".
            // Returns query/users
            IInvocationOperation call
                => GetCallCrossings(value.Position.FlowGraph.Bridges.FindFromResult(call.TargetMethod), call),

            // "users[0]", "enumerator.Current", "pair.Value", "task.Result": the entities come from the object
            // the property is read on. Returns users/enumerator/pair/page
            IPropertyReferenceOperation reference
                when value.Position.FlowGraph.Bridges.FindFromResult(reference.Property)
                    .Any(bridge => bridge.To is BridgeEnd.Instance)
                => Single(reference.Instance),

            // "users[0]" of an array. Roslyn exposes nothing to name here, so no annotation can describe
            // it, and there is only one value the element can come from. Returns users
            IArrayElementReferenceOperation element
                => Single(element.ArrayReference),

            _ => [],
        };

    /// <summary>
    /// The collection a lambda parameter is filled from: for the <c>u</c> of <c>users.Select(u =&gt; ...)</c>,
    /// the value <c>users</c>. Empty for a parameter nobody annotated, such as the index of
    /// <c>Select((u, i) =&gt; ...)</c>.
    /// </summary>
    /// <param name="lambda">Lambda the parameter belongs to.</param>
    /// <param name="parameterOrdinal">Position of the parameter in the lambda.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    /// <returns>Crossings.</returns>
    public static IReadOnlyList<Crossing> FromLambdaParameter(
        IFlowAnonymousFunctionOperation lambda,
        int parameterOrdinal,
        Bridges bridges)
    {
        if (GetLambdaArgument(lambda) is not { Parameter: { } lambdaArgument, Parent: IInvocationOperation call })
        {
            return [];
        }

        // The bridge names the callback and the position inside it, so no overload check is needed.
        return GetCallCrossings(
            bridges.FindFromParameter(call.TargetMethod, lambdaArgument.Name, parameterOrdinal),
            call);
    }

    /// <summary>
    /// The argument a lambda was passed as: the <c>u =&gt; ...</c> of <c>users.Select(u =&gt; ...)</c>. Null for a
    /// lambda that is not an argument, such as one assigned to a variable.
    /// </summary>
    /// <param name="lambda">Lambda.</param>
    /// <returns>Argument, or null.</returns>
    public static IArgumentOperation? GetLambdaArgument(IFlowAnonymousFunctionOperation lambda)
    {
        var parent = lambda.Parent;
        while (parent is IConversionOperation or IDelegateCreationOperation)
        {
            parent = parent.Parent;
        }

        return parent as IArgumentOperation;
    }

    /// <summary>
    /// The value an out argument came from: for <c>d.TryGetValue(k, out var user)</c>, the value <c>d</c>.
    /// </summary>
    /// <param name="call">Call that wrote the out argument.</param>
    /// <param name="outParameter">Name of the out parameter the entities arrived at.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    /// <returns>Crossings.</returns>
    public static IReadOnlyList<Crossing> FromOutArgument(
        IInvocationOperation call,
        string outParameter,
        Bridges bridges)
        => GetCallCrossings(bridges.FindFromParameter(call.TargetMethod, outParameter), call);

    /// <summary>
    /// The crossings of the bridges at the call. Empty when any of them cannot be crossed: the entities could
    /// have come from that place too, and checking only the others would answer for it without looking.
    /// </summary>
    private static IReadOnlyList<Crossing> GetCallCrossings(
        IReadOnlyList<Bridge> bridges,
        IInvocationOperation call)
    {
        var crossings = new List<Crossing>(bridges.Count);

        foreach (var bridge in bridges)
        {
            if (GetCallCrossing(bridge, call) is not { } crossing)
            {
                return [];
            }

            crossings.Add(crossing);
        }

        return crossings;
    }

    /// <summary>
    /// The one value there is, with the path kept, or nothing when there is none.
    /// </summary>
    private static IReadOnlyList<Crossing> Single(IOperation? source)
        => source is null ? [] : [new Crossing(source, [])];

    /// <summary>
    /// Crosses one bridge at the call: the value at the end it lands on, and what its step puts in front of the
    /// path. Null when the value is not there, or the step cannot be read at this call.
    /// </summary>
    private static Crossing? GetCallCrossing(Bridge bridge, IInvocationOperation call)
    {
        var source = GetCallSource(bridge.To, call);
        if (source is null)
        {
            return null;
        }

        var prefix = GetPrefix(bridge.Step, call);
        if (prefix is null)
        {
            return null;
        }

        return new Crossing(source, prefix);
    }

    /// <summary>
    /// The value at the end a bridge lands on: what the method was called on, or the argument for a named
    /// parameter. A bridge never lands on a result.
    /// </summary>
    private static IOperation? GetCallSource(BridgeEnd to, IInvocationOperation call)
        => to switch
        {
            BridgeEnd.Instance => RoslynReader.GetReceiver(call),
            BridgeEnd.Parameter parameter => GetArgument(call, parameter.Name),
            _ => null,
        };

    /// <summary>
    /// What the step puts in front of the path at this call. Null for a selector that is not a member chain,
    /// such as <c>u =&gt; u.Manager ?? u</c>: the bridge is then not crossed.
    /// </summary>
    private static IReadOnlyList<string>? GetPrefix(PathStep step, IInvocationOperation call)
        => step switch
        {
            PathStep.KeepStep => [],
            PathStep.SelectorChainStep selector
                => GetArgument(call, selector.ParameterName) is { } argument
                    ? ReadSelectorChain(argument.Syntax)
                    : null,
            _ => null,
        };

    /// <summary>
    /// The properties a selector reads from its element, the first parameter: ["Settings"] for
    /// <c>u =&gt; u.Settings</c> and for <c>(u, i) =&gt; u.Settings</c>.
    /// </summary>
    private static IReadOnlyList<string>? ReadSelectorChain(SyntaxNode lambda)
    {
        var parameter = RoslynReader.GetLambdaParameter(lambda, position: 0);
        var body = RoslynReader.GetLambdaBody(lambda);

        return parameter is null || body is null
            ? null
            : RoslynReader.ReadMemberChain(body, parameter);
    }

    /// <summary>
    /// The value passed for the named parameter.
    /// </summary>
    private static IOperation? GetArgument(IInvocationOperation call, string parameterName)
        => call.Arguments
            .FirstOrDefault(argument => argument.Parameter?.Name == parameterName)
            ?.Value;
}
