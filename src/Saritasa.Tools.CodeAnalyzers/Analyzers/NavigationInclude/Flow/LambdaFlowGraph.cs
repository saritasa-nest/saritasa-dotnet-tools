using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Graph of a lambda body. The lambda body is a separate graph; the statement that creates the lambda only holds
/// a reference to it.
/// </summary>
internal sealed class LambdaFlowGraph : IFlowGraph
{
    /// <summary>
    /// Initializes the graph.
    /// </summary>
    /// <param name="lambda">Lambda.</param>
    /// <param name="createdAt">Statement that creates the lambda.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public LambdaFlowGraph(IFlowAnonymousFunctionOperation lambda, CodePosition createdAt, CancellationToken cancellationToken)
    {
        Graph = createdAt.FlowGraph.Graph.GetAnonymousFunctionControlFlowGraph(lambda, cancellationToken);
        Lambda = lambda;
        CreatedAt = createdAt;
    }

    /// <inheritdoc />
    public ControlFlowGraph Graph { get; }

    /// <inheritdoc />
    public IMethodSymbol Method => Lambda.Symbol;

    /// <summary>
    /// The lambda.
    /// </summary>
    public IFlowAnonymousFunctionOperation Lambda { get; }

    /// <summary>
    /// The statement that creates the lambda. Variables the lambda captures get their values before it.
    /// </summary>
    public CodePosition CreatedAt { get; }

    /// <inheritdoc />
    public IFlowAnonymousFunctionOperation? FindLambda(ISymbol lambdaSymbol)
        => SymbolEqualityComparer.Default.Equals(Lambda.Symbol, lambdaSymbol)
            ? Lambda

            // "users.Select(u => u.Orders.Select(o => u))": continue in the graph that creates this lambda.
            : CreatedAt.FlowGraph.FindLambda(lambdaSymbol);
}
