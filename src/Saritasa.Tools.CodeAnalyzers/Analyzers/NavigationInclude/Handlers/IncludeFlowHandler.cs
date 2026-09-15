using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;

/// <summary>
/// Entry point of the flow analysis: called once per method body, reports INCL001 (call sites),
/// INCL002 and INCL003 through <see cref="IncludeRulesChecker"/>.
/// </summary>
internal static class IncludeFlowHandler
{
    /// <summary>
    /// Analyzes one method body.
    /// </summary>
    /// <param name="context">Operation block analysis context.</param>
    public static void Analyze(OperationBlockAnalysisContext context)
    {
        if (context.OwningSymbol is not IMethodSymbol method)
        {
            return;
        }

        var body = FindMethodBody(context);
        if (body is null)
        {
            return;
        }

        // Roslyn splits the method into blocks for us.
        var graph = context.GetControlFlowGraph(body);
        AnalyzeMethod(context, method, graph);
    }

    private static void AnalyzeMethod(OperationBlockAnalysisContext context, IMethodSymbol method, ControlFlowGraph graph)
    {
        var checker = new IncludeRulesChecker(context, method);
        checker.CheckGraph(graph, IncludeRulesChecker.CreateTableAtMethodStart(method), isLambda: false);

        // Local functions have their own graph and their own attributes.
        foreach (var localFunction in graph.LocalFunctions)
        {
            var localFunctionGraph = graph.GetLocalFunctionControlFlowGraph(localFunction, context.CancellationToken);
            AnalyzeMethod(context, localFunction, localFunctionGraph);
        }
    }

    /// <summary>
    /// Returns the block that belongs to the method or constructor body.
    /// Other blocks (attribute arguments, parameter default values) have no control flow to analyze.
    /// </summary>
    private static IOperation? FindMethodBody(OperationBlockAnalysisContext context)
    {
        foreach (var block in context.OperationBlocks)
        {
            var root = block;
            while (root.Parent is not null)
            {
                root = root.Parent;
            }

            if (root is IMethodBodyOperation or IConstructorBodyOperation)
            {
                return block;
            }
        }

        return null;
    }
}
