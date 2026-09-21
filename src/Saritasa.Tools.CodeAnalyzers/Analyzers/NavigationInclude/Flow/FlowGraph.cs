using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// One body being analyzed — a method, a constructor, a local function or a lambda — together with its control
/// flow graph: the code split into blocks connected by jumps.
/// </summary>
/// <remarks>
/// A lambda body is a graph of its own; the statement that creates the lambda only holds a reference to it.
/// <see cref="CreationStatement"/> is that statement, and it is null for every other kind of body.
/// </remarks>
internal sealed class FlowGraph
{
    /// <summary>
    /// Initializes the graph of a method, constructor or local function body.
    /// </summary>
    /// <param name="graph">Control flow graph of the body.</param>
    /// <param name="method">Method, constructor or local function.</param>
    /// <param name="declarations">Every [PassesIncludes] the compilation can see.</param>
    public FlowGraph(ControlFlowGraph graph, IMethodSymbol method, IncludeDeclarations declarations)
    {
        Graph = graph;
        Method = method;
        Declarations = declarations;
    }

    private FlowGraph(
        IFlowAnonymousFunctionOperation lambda,
        CodePosition creationStatement,
        CancellationToken cancellationToken)
    {
        Graph = creationStatement.FlowGraph.Graph.GetAnonymousFunctionControlFlowGraph(lambda, cancellationToken);
        Method = lambda.Symbol;
        CreationStatement = creationStatement;
        Declarations = creationStatement.FlowGraph.Declarations;
        Lambda = lambda;
    }

    /// <summary>
    /// Control flow graph of the body.
    /// </summary>
    public ControlFlowGraph Graph { get; }

    /// <summary>
    /// Method, constructor, local function or lambda whose body the graph is.
    /// </summary>
    public IMethodSymbol Method { get; }

    /// <summary>
    /// The statement that creates the lambda whose body this graph is; null for every other kind of body.
    /// The variables the lambda captures get their values before that statement.
    /// </summary>
    public CodePosition? CreationStatement { get; }

    /// <summary>
    /// Every [PassesIncludes] the compilation can see. The same one for every body of a compilation.
    /// </summary>
    public IncludeDeclarations Declarations { get; }

    /// <summary>
    /// The lambda whose body the graph is; null for every other kind of body.
    /// </summary>
    public IFlowAnonymousFunctionOperation? Lambda { get; }

    /// <summary>
    /// True if the graph is the body of a lambda.
    /// </summary>
    public bool IsLambda => Lambda is not null;

    /// <summary>
    /// Returns the reachable statements of the graph and of the lambdas and local functions declared in it.
    /// If/while conditions and returned values count as statements.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Positions of the statements.</returns>
    public IEnumerable<CodePosition> GetStatements(CancellationToken cancellationToken)
    {
        var ownStatements = Graph.Blocks
            .Where(block => block.IsReachable)
            .SelectMany(block => CodePosition.GetBlockStatements(this, block));

        var lambdaStatements = ownStatements
            .SelectMany(position => GetLambdaGraphs(position, cancellationToken))
            .SelectMany(lambdaGraph => lambdaGraph.GetStatements(cancellationToken));

        var localFunctionStatements = Graph.LocalFunctions
            .Select(localFunction => new FlowGraph(
                Graph.GetLocalFunctionControlFlowGraph(localFunction, cancellationToken),
                localFunction,
                Declarations))
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
        if (Lambda is null || CreationStatement is null)
        {
            // A method, constructor or local function has no lambdas around it.
            return null;
        }

        return SymbolEqualityComparer.Default.Equals(Lambda.Symbol, lambdaSymbol)
            ? Lambda

            // "users.Select(u => u.Orders.Select(o => u))": continue in the graph that creates this lambda.
            : CreationStatement.FlowGraph.FindLambda(lambdaSymbol);
    }

    /// <summary>
    /// The graphs of the lambda bodies the statement creates.
    /// </summary>
    private static IEnumerable<FlowGraph> GetLambdaGraphs(CodePosition position, CancellationToken cancellationToken)
    {
        if (position.Statement is not { } statement)
        {
            return [];
        }

        return statement
            .DescendantsAndSelf()
            .OfType<IFlowAnonymousFunctionOperation>()
            .Select(lambda => new FlowGraph(lambda, position, cancellationToken));
    }
}
