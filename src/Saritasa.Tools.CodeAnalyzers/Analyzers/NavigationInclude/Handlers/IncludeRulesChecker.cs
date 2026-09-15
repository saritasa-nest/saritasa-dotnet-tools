using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;

/// <summary>
/// Walks a control flow graph with the calculated <see cref="LoadedProperties"/> tables and reports
/// INCL001 (call sites), INCL002 and INCL003.
/// </summary>
internal sealed class IncludeRulesChecker
{
    private readonly OperationBlockAnalysisContext context;

    /// <summary>
    /// The method (or local function) whose parameters and attributes apply.
    /// </summary>
    private readonly IMethodSymbol method;

    /// <summary>
    /// Initializes the checker for one method or local function.
    /// </summary>
    /// <param name="context">Analysis context used to report diagnostics.</param>
    /// <param name="method">Method whose parameters and attributes apply.</param>
    public IncludeRulesChecker(OperationBlockAnalysisContext context, IMethodSymbol method)
    {
        this.context = context;
        this.method = method;
    }

    /// <summary>
    /// Creates the table at the start of the method: every parameter has the properties its
    /// <see cref="IncludeRequiredAttribute"/> guarantees.
    /// </summary>
    /// <param name="method">Method.</param>
    /// <returns>Table at the start of the method.</returns>
    public static LoadedProperties CreateTableAtMethodStart(IMethodSymbol method)
    {
        var table = LoadedProperties.Empty;
        foreach (var parameter in method.Parameters)
        {
            table = table.SetProperties(parameter, LoadedProperties.NoProperties);
        }

        foreach (var attribute in method.GetAttributes())
        {
            if (!AttributeHelper.TryGetIncludeRequiredArgs(attribute, out var parameterName, out var propertyName))
            {
                continue;
            }

            var parameter = method.Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter is not null)
            {
                table = table.AddProperty(parameter, propertyName!);
            }
        }

        return table;
    }

    /// <summary>
    /// Calculates the tables for the graph, then checks every block.
    /// </summary>
    /// <param name="graph">Control flow graph of the method or of a lambda inside it.</param>
    /// <param name="tableAtStart">Table at the start of the graph.</param>
    /// <param name="isLambda">True for lambdas: their return values are not checked by INCL003.</param>
    public void CheckGraph(ControlFlowGraph graph, LoadedProperties tableAtStart, bool isLambda)
    {
        var tablesAtBlockStart = BlockStartCalculator.CalculateTableAtStartOfEachBlock(graph, tableAtStart);

        foreach (var block in graph.Blocks)
        {
            if (block.IsReachable)
            {
                CheckBlock(graph, block, tablesAtBlockStart[block.Ordinal], isLambda);
            }
        }
    }

    private void CheckBlock(ControlFlowGraph graph, BasicBlock block, LoadedProperties table, bool isLambda)
    {
        foreach (var statement in block.Operations)
        {
            CheckStatement(graph, statement, table);
            table = StatementEffects.UpdateTableForStatement(statement, table);
        }

        // The branch value is an if/while condition or the value after "return".
        if (block.BranchValue is null)
        {
            return;
        }

        CheckStatement(graph, block.BranchValue, table);

        var isReturn = block.FallThroughSuccessor?.Semantics == ControlFlowBranchSemantics.Return;
        if (isReturn && !isLambda)
        {
            CheckReturnedValue(block.BranchValue, table);
        }
    }

    private void CheckStatement(ControlFlowGraph graph, IOperation statement, LoadedProperties table)
    {
        // One statement can contain several calls: users.Select(u => ...).ToList().
        foreach (var call in statement.DescendantsAndSelf().OfType<IInvocationOperation>())
        {
            CheckCallArguments(call, table);

            foreach (var argument in call.Arguments)
            {
                if (ExpressionLoadedProperties.SkipConversions(argument.Value) is IFlowAnonymousFunctionOperation lambda)
                {
                    CheckLambda(graph, call, lambda, table);
                }
            }
        }
    }

    /// <summary>
    /// INCL001 / INCL002: the called method requires a property for an argument - is it loaded.
    /// </summary>
    private void CheckCallArguments(IInvocationOperation call, LoadedProperties table)
    {
        foreach (var attribute in call.TargetMethod.GetAttributes())
        {
            if (!AttributeHelper.TryGetIncludeRequiredArgs(attribute, out var parameterName, out var propertyName))
            {
                continue;
            }

            var argument = call.Arguments.FirstOrDefault(a => a.Parameter?.Name == parameterName);
            if (argument is null)
            {
                continue;
            }

            string ruleId;
            ISymbol variable;
            ITypeSymbol variableType;
            var passedValue = ExpressionLoadedProperties.SkipConversions(argument.Value);
            if (passedValue is ILocalReferenceOperation local)
            {
                ruleId = NavigationIncludeRulesProvider.RuleIncl2Id;
                variable = local.Local;
                variableType = local.Local.Type;
            }
            else if (passedValue is IParameterReferenceOperation parameter &&
                     SymbolEqualityComparer.Default.Equals(parameter.Parameter.ContainingSymbol, method))
            {
                // The method's own parameter: the fix is to declare [IncludeRequired] on this method.
                ruleId = NavigationIncludeRulesProvider.RuleIncl1Id;
                variable = parameter.Parameter;
                variableType = parameter.Parameter.Type;
            }
            else if (passedValue is IParameterReferenceOperation lambdaParameter && table.IsTracked(lambdaParameter.Parameter))
            {
                // A lambda parameter with known properties: users.Select(u => ...).
                ruleId = NavigationIncludeRulesProvider.RuleIncl2Id;
                variable = lambdaParameter.Parameter;
                variableType = lambdaParameter.Parameter.Type;
            }
            else
            {
                continue;
            }

            if (table.GetProperties(variable).Contains(propertyName!))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    NavigationIncludeRulesProvider.GetDiagnosticDescriptor(ruleId),
                    call.Syntax.GetLocation(),
                    variableType.Name,
                    propertyName,
                    variable.Name));
        }
    }

    /// <summary>
    /// INCL003: a method with <see cref="IncludesAttribute"/> must return a value with the property loaded,
    /// unless verification is disabled with <c>Verify = false</c>.
    /// </summary>
    private void CheckReturnedValue(IOperation returnedValue, LoadedProperties table)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (!AttributeHelper.TryGetIncludesArg(attribute, out var propertyName) ||
                !AttributeHelper.IsIncludesVerificationEnabled(attribute))
            {
                continue;
            }

            if (ExpressionLoadedProperties.Get(returnedValue, table).Contains(propertyName!))
            {
                continue;
            }

            // The graph keeps only the returned expression; report on the whole "return ...;" statement.
            var syntax = returnedValue.Syntax.Parent as ReturnStatementSyntax ?? returnedValue.Syntax;
            context.ReportDiagnostic(
                Diagnostic.Create(
                    NavigationIncludeRulesProvider.GetDiagnosticDescriptor(NavigationIncludeRulesProvider.RuleIncl3Id),
                    syntax.GetLocation(),
                    propertyName));
        }
    }

    /// <summary>
    /// A lambda has its own control flow graph. It starts with the table of the place where it is created,
    /// so captured variables keep their properties.
    /// </summary>
    private void CheckLambda(
        ControlFlowGraph graph,
        IInvocationOperation call,
        IFlowAnonymousFunctionOperation lambda,
        LoadedProperties table)
    {
        var tableInsideLambda = table;

        // users.Select(u => ...): u is an element of users and has the same loaded properties.
        if (ExpressionLoadedProperties.PassesElementsToLambda(call) && lambda.Symbol.Parameters.Length > 0)
        {
            var elementProperties = ExpressionLoadedProperties.Get(ExpressionLoadedProperties.GetSource(call), table);
            tableInsideLambda = table.SetProperties(lambda.Symbol.Parameters[0], elementProperties);
        }

        var lambdaGraph = graph.GetAnonymousFunctionControlFlowGraph(lambda, context.CancellationToken);
        CheckGraph(lambdaGraph, tableInsideLambda, isLambda: true);
    }
}
