using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// One control flow graph being analyzed: a method body or a local function (<see cref="MethodFlowGraph"/>),
/// or a lambda (<see cref="LambdaFlowGraph"/>).
/// </summary>
internal interface IFlowGraph
{
    /// <summary>
    /// Control flow graph: the code split into blocks connected by jumps.
    /// </summary>
    ControlFlowGraph Graph { get; }

    /// <summary>
    /// Method, local function or lambda whose body the graph is.
    /// </summary>
    IMethodSymbol Method { get; }

    /// <summary>
    /// Returns the lambda with the symbol: this graph's lambda or a lambda around it. Null if not found.
    /// </summary>
    /// <param name="lambdaSymbol">Lambda symbol.</param>
    /// <returns>Lambda or null.</returns>
    IFlowAnonymousFunctionOperation? FindLambda(ISymbol lambdaSymbol);
}
