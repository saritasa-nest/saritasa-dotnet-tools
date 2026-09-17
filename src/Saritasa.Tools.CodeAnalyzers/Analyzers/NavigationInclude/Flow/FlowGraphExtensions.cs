using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Operations common for all <see cref="IFlowGraph"/> kinds.
/// </summary>
internal static class FlowGraphExtensions
{
    /// <summary>
    /// Returns the reachable statements of the graph and of the lambdas and local functions declared in it.
    /// If/while conditions and returned values count as statements.
    /// </summary>
    /// <param name="flowGraph">Graph.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Positions of the statements.</returns>
    public static IEnumerable<CodePosition> GetStatements(this IFlowGraph flowGraph, CancellationToken cancellationToken)
    {
        var ownStatements = flowGraph.Graph.Blocks
            .Where(block => block.IsReachable)
            .SelectMany(block => CodePosition.AllInBlock(flowGraph, block));

        // A lambda body is a separate graph; the statement only holds a reference to it.
        var lambdaStatements = ownStatements
            .SelectMany(position => position.Statement
                .DescendantsAndSelf()
                .OfType<IFlowAnonymousFunctionOperation>()
                .Select(lambda => new LambdaFlowGraph(lambda, position, cancellationToken)))
            .SelectMany(lambdaGraph => lambdaGraph.GetStatements(cancellationToken));

        var localFunctionStatements = flowGraph.Graph.LocalFunctions
            .Select(localFunction => new MethodFlowGraph(
                flowGraph.Graph.GetLocalFunctionControlFlowGraph(localFunction, cancellationToken),
                localFunction))
            .SelectMany(localFunctionGraph => localFunctionGraph.GetStatements(cancellationToken));

        return ownStatements.Concat(lambdaStatements).Concat(localFunctionStatements);
    }
}
