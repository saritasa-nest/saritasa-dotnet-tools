using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Graph of a method body or a local function.
/// </summary>
internal sealed class MethodFlowGraph : IFlowGraph
{
    /// <summary>
    /// Initializes the graph.
    /// </summary>
    /// <param name="graph">Control flow graph of the body.</param>
    /// <param name="method">Method or local function.</param>
    public MethodFlowGraph(ControlFlowGraph graph, IMethodSymbol method)
    {
        Graph = graph;
        Method = method;
    }

    /// <inheritdoc />
    public ControlFlowGraph Graph { get; }

    /// <inheritdoc />
    public IMethodSymbol Method { get; }

    /// <inheritdoc />
    /// <remarks>A method or local function has no lambdas around it.</remarks>
    public IFlowAnonymousFunctionOperation? FindLambda(ISymbol lambdaSymbol) => null;
}
