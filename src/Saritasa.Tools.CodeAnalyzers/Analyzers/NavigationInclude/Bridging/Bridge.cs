using System.Diagnostics;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// One annotated move of entities across a call: they sit at <see cref="From"/>, and the same entities came
/// from <see cref="To"/>.
/// </summary>
/// <remarks>
/// The search reads backwards, so a bridge is written that way too. It says nothing about which property is
/// loaded, only that the entities on both sides are the same ones, which is why one bridge covers every
/// property. The member it belongs to is the key it is stored under, see <see cref="Bridges"/>.
/// Only four pairs are meaningful. The search always stands on a result or on something that arrived through
/// a parameter, so <see cref="From"/> is never an instance; it only moves backwards, so <see cref="To"/> is
/// never a result.
/// <list type="bullet">
/// <item><description>result to instance: <c>users.ToList()</c>, <c>users[0]</c>.</description></item>
/// <item><description>result to parameter: <c>Enumerable.ToList(users)</c>.</description></item>
/// <item><description>parameter to instance: <c>dictionary.TryGetValue(id, out var user)</c>.</description></item>
/// <item><description>parameter to parameter: <c>Enumerable.Select(query, selector)</c>.</description></item>
/// </list>
/// A bridge somebody wrote may only describe a static method, see <see cref="CustomBridgeRules"/>. Landing
/// on the instance is still open to it, because for an extension method the value in front of the dot is
/// simply its first parameter.
/// </remarks>
internal sealed class Bridge
{
    /// <summary>
    /// Initializes the bridge.
    /// </summary>
    /// <param name="from">Where the entities sit when the search reaches them.</param>
    /// <param name="to">Where the same entities came from.</param>
    public Bridge(BridgeEnd from, BridgeEnd to)
    {
        Debug.Assert(from is not BridgeEnd.Instance, "A bridge never runs from the value a member was used on.");
        Debug.Assert(to is not BridgeEnd.Result, "A bridge never lands on a result; the search reads backwards.");

        From = from;
        To = to;
    }

    /// <summary>
    /// Where the entities sit when the search reaches them: a result, or a parameter they travelled through.
    /// </summary>
    public BridgeEnd From { get; }

    /// <summary>
    /// Where the same entities came from: the value the member was used on, or one of its parameters.
    /// </summary>
    public BridgeEnd To { get; }
}
