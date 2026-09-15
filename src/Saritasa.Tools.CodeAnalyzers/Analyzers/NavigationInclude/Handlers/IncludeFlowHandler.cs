using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;

/// <summary>
/// Finds every call of an [IncludeRequired] method and every return of an [Includes] method,
/// and asks <see cref="LoadedPropertySearch"/> whether the property is loaded there.
/// Reports INCL001 (call sites), INCL002 and INCL003.
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

        var diagnostics = FlowGraph
            .ForMethod(context.GetControlFlowGraph(body), method)
            .GetStatements(context.CancellationToken)
            .SelectMany(position => FindCallDiagnostics(position).Concat(FindReturnDiagnostics(position)));

        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// INCL001 / INCL002: for each [IncludeRequired(parameter, property)] of a called method,
    /// the passed value must have the property loaded.
    /// </summary>
    private static IEnumerable<Diagnostic> FindCallDiagnostics(CodePosition statementPosition)
    {
        var methodCalls = statementPosition.Statement
            !.DescendantsAndSelf()
            .OfType<IInvocationOperation>();

        foreach (var call in methodCalls)
        {
            var methodIncludeRequirements = AttributeHelper.GetIncludeRequirements(call.TargetMethod);
            foreach (var (parameterName, propertyToInclude) in methodIncludeRequirements)
            {
                var argument = call.Arguments.FirstOrDefault(a => a.Parameter?.Name == parameterName);
                if (argument is null)
                {
                    continue;
                }

                var passedValue = LoadedPropertySearch.SkipConversions(argument.Value)!;
                var ruleId = ChooseRule(passedValue, statementPosition.FlowGraph);
                if (ruleId is null || LoadedPropertySearch.IsLoaded(passedValue, propertyToInclude, statementPosition))
                {
                    continue;
                }

                yield return Diagnostic.Create(
                    NavigationIncludeRulesProvider.GetDiagnosticDescriptor(ruleId),
                    call.Syntax.GetLocation(),
                    passedValue.Type?.Name,
                    propertyToInclude,
                    passedValue.Syntax.ToString());
            }
        }
    }

    /// <summary>
    /// INCL003: for each [Includes(property)] of the method, the returned value must have the property loaded,
    /// unless the attribute is declared with <c>Verify = false</c>.
    /// </summary>
    private static IEnumerable<Diagnostic> FindReturnDiagnostics(CodePosition position)
    {
        if (!position.IsReturnFromMethod)
        {
            yield break;
        }

        var returnedValue = position.Statement!;

        // The graph keeps only the returned expression; report on the whole "return ...;" statement.
        var location = (returnedValue.Syntax.Parent as ReturnStatementSyntax ?? returnedValue.Syntax).GetLocation();

        foreach (var property in AttributeHelper.GetVerifiedIncludes(position.FlowGraph.Method))
        {
            if (!LoadedPropertySearch.IsLoaded(returnedValue, property, position))
            {
                yield return Diagnostic.Create(
                    NavigationIncludeRulesProvider.GetDiagnosticDescriptor(NavigationIncludeRulesProvider.RuleIncl3Id),
                    location,
                    property);
            }
        }
    }

    /// <summary>
    /// INCL001 for a method parameter (fix: declare [IncludeRequired] on the method),
    /// INCL002 for a local or a LINQ lambda parameter (fix: include the property),
    /// null for values that are not checked (fields, method calls, other lambda parameters).
    /// </summary>
    private static string? ChooseRule(IOperation passedValue, FlowGraph flowGraph)
        => passedValue switch
        {
            ILocalReferenceOperation => NavigationIncludeRulesProvider.RuleIncl2Id,

            IParameterReferenceOperation { Parameter.ContainingSymbol: IMethodSymbol { MethodKind: not MethodKind.AnonymousFunction } }
                => NavigationIncludeRulesProvider.RuleIncl1Id,

            // u of users.Select(u => ...).
            IParameterReferenceOperation { Parameter: { Ordinal: 0 } parameter }
                when flowGraph.FindLambda(parameter.ContainingSymbol) is { } lambda &&
                     LinqMethods.GetElementSource(lambda) is not null
                => NavigationIncludeRulesProvider.RuleIncl2Id,

            _ => null,
        };

    /// <summary>
    /// Returns the block that belongs to the method or constructor body.
    /// Other blocks (attribute arguments, parameter default values) have no control flow to analyze.
    /// </summary>
    private static IOperation? FindMethodBody(OperationBlockAnalysisContext context)
        => context.OperationBlocks.FirstOrDefault(block => GetRoot(block) is IMethodBodyOperation or IConstructorBodyOperation);

    private static IOperation GetRoot(IOperation operation)
        => operation.Parent is null ? operation : GetRoot(operation.Parent);
}
