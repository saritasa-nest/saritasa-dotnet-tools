using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// One control flow graph being analyzed: a method body, a local function or a lambda.
/// </summary>
internal sealed class FlowGraph
{
    private FlowGraph(
        ControlFlowGraph graph,
        IMethodSymbol method,
        IFlowAnonymousFunctionOperation? lambda,
        CodePosition? lambdaCreatedAt)
    {
        Graph = graph;
        Method = method;
        Lambda = lambda;
        LambdaCreatedAt = lambdaCreatedAt;
    }

    /// <summary>
    /// Control flow graph: the code split into blocks connected by jumps.
    /// </summary>
    public ControlFlowGraph Graph { get; }

    /// <summary>
    /// Method, local function or lambda whose body the graph is.
    /// </summary>
    public IMethodSymbol Method { get; }

    /// <summary>
    /// For a lambda graph: the lambda. Null for methods and local functions.
    /// </summary>
    public IFlowAnonymousFunctionOperation? Lambda { get; }

    /// <summary>
    /// For a lambda graph: the statement that creates the lambda. Variables the lambda captures
    /// get their values before it.
    /// </summary>
    public CodePosition? LambdaCreatedAt { get; }

    /// <summary>
    /// Creates the graph of a method or local function.
    /// </summary>
    /// <param name="graph">Control flow graph of the body.</param>
    /// <param name="method">Method or local function.</param>
    /// <returns>Graph.</returns>
    public static FlowGraph ForMethod(ControlFlowGraph graph, IMethodSymbol method)
        => new(graph, method, lambda: null, lambdaCreatedAt: null);

    /// <summary>
    /// Creates the graph of a lambda body.
    /// </summary>
    /// <param name="lambda">Lambda.</param>
    /// <param name="createdAt">Statement that creates the lambda.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Graph.</returns>
    public static FlowGraph ForLambda(IFlowAnonymousFunctionOperation lambda, CodePosition createdAt, CancellationToken cancellationToken)
        => new(
            createdAt.FlowGraph.Graph.GetAnonymousFunctionControlFlowGraph(lambda, cancellationToken),
            lambda.Symbol,
            lambda,
            createdAt);

    /// <summary>
    /// Returns the reachable statements of this graph and of the lambdas and local functions declared in it.
    /// If/while conditions and returned values count as statements.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Positions of the statements.</returns>
    public IEnumerable<CodePosition> GetStatements(CancellationToken cancellationToken)
    {
        var ownStatements = Graph.Blocks
            .Where(block => block.IsReachable)
            .SelectMany(block => Enumerable
                .Range(0, block.Operations.Length + 1)
                .Select(statementIndex => new CodePosition(this, block, statementIndex)))
            .Where(position => position.Statement is not null);

        // A lambda body is a separate graph; the statement only holds a reference to it.
        var lambdaStatements = ownStatements
            .SelectMany(position => position.Statement!
                .DescendantsAndSelf()
                .OfType<IFlowAnonymousFunctionOperation>()
                .Select(lambda => ForLambda(lambda, position, cancellationToken)))
            .SelectMany(lambdaGraph => lambdaGraph.GetStatements(cancellationToken));

        var localFunctionStatements = Graph.LocalFunctions
            .Select(localFunction => ForMethod(
                Graph.GetLocalFunctionControlFlowGraph(localFunction, cancellationToken),
                localFunction))
            .SelectMany(localFunctionGraph => localFunctionGraph.GetStatements(cancellationToken));

        return ownStatements.Concat(lambdaStatements).Concat(localFunctionStatements);
    }

    /// <summary>
    /// Returns the lambda with the symbol: this graph's lambda or a lambda around it. Null if not found.
    /// </summary>
    /// <param name="lambdaSymbol">Lambda symbol.</param>
    /// <returns>Lambda or null.</returns>
    public IFlowAnonymousFunctionOperation? FindLambda(ISymbol lambdaSymbol)
    {
        for (var graph = this; graph.Lambda is not null && graph.LambdaCreatedAt is not null; graph = graph.LambdaCreatedAt.FlowGraph)
        {
            if (SymbolEqualityComparer.Default.Equals(graph.Lambda.Symbol, lambdaSymbol))
            {
                return graph.Lambda;
            }
        }

        return null;
    }
}
