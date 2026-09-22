using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Roslyn;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;

/// <summary>
/// Finds every call of an [IncludeRequired] method and every return of an [Includes] method,
/// and asks <see cref="IncludeSearcher"/> whether the property is loaded there.
/// Reports INCL001 (call sites), INCL002 and INCL003.
/// </summary>
internal static class IncludeFlowHandler
{
    /// <summary>
    /// Analyzes one method body.
    /// </summary>
    /// <param name="context">Operation block analysis context.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    public static void Analyze(OperationBlockAnalysisContext context, Bridges bridges)
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

        var diagnostics = new FlowGraph(context.GetControlFlowGraph(body), method, bridges)
            .GetStatements(context.CancellationToken)
            .SelectMany(position =>
                GetMethodCallDiagnostics(position).Concat(GetReturnDiagnostics(position)));

        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// INCL001 / INCL002: for each [IncludeRequired(parameter, property)] of a called method,
    /// the passed value must have the property loaded.
    /// </summary>
    private static IEnumerable<Diagnostic> GetMethodCallDiagnostics(CodePosition statementPosition)
    {
        if (statementPosition.Statement is not { } statement)
        {
            return [];
        }

        return statement
            .DescendantsAndSelf()
            .OfType<IInvocationOperation>()
            .SelectMany(call => GetCallDiagnostics(call, statementPosition))

            // The same requirement declared twice is still one problem.
            .Distinct();
    }

    /// <summary>
    /// Checks one call against every [IncludeRequired] of the method it calls.
    /// </summary>
    private static IEnumerable<Diagnostic> GetCallDiagnostics(
        IInvocationOperation call,
        CodePosition callStatement)
    {
        foreach (var requirement in AttributeReader.GetIncludeRequirements(call.TargetMethod))
        {
            var argument = call.Arguments.FirstOrDefault(argument =>
                argument.Parameter?.Name == requirement.ParameterName);

            if (argument is null)
            {
                // Argument is not passed yet for this parameter.
                continue;
            }

            // "Process(user)" with a parameter of a base type, interface or nullable type wraps the local into
            // a conversion. The rule matches the local itself, and the diagnostic reports its own type.
            var value = RoslynReader.SkipWrappers(argument.Value);

            var ruleId = GetRuleForArgument(value, callStatement.FlowGraph);
            if (ruleId is null)
            {
                continue;
            }

            var answer = IncludeSearcher.Check(value, requirement.NavigationProperty, callStatement);
            if (answer.IsLoaded)
            {
                continue;
            }

            // The search could not read the whole path, so report that instead of a mistake in the code.
            if (answer.IsUnknown)
            {
                yield return CannotCheck(
                    call.Syntax.GetLocation(),
                    answer.UnreadableValue,
                    value,
                    requirement.NavigationProperty);

                continue;
            }

            yield return Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(ruleId),
                call.Syntax.GetLocation(),
                value.Type?.Name,
                requirement.NavigationProperty,
                value.Syntax.ToString());
        }
    }

    /// <summary>
    /// INCL003: for each [Includes(property)] of the method, the returned value must have the property loaded,
    /// unless the attribute is declared with <c>Verify = false</c>.
    /// </summary>
    private static IEnumerable<Diagnostic> GetReturnDiagnostics(CodePosition position)
    {
        if (!position.IsReturnFromMethod || position.Statement is not { } returnedValue)
        {
            yield break;
        }

        foreach (var property in AttributeReader.GetIncludesToVerify(position.FlowGraph.Method))
        {
            var answer = IncludeSearcher.Check(returnedValue, property, position);
            if (answer.IsLoaded)
            {
                continue;
            }

            // The graph keeps only the returned expression; report on the whole "return ...;" statement.
            var location = (returnedValue.Syntax.Parent as ReturnStatementSyntax ?? returnedValue.Syntax).GetLocation();

            if (answer.IsUnknown)
            {
                yield return CannotCheck(location, answer.UnreadableValue, returnedValue, property);

                continue;
            }

            yield return Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                    NavigationIncludeRulesProvider.Incl3IdMethodResultDoesntIncludeNavigationProperty),
                location,
                property);
        }
    }

    /// <summary>
    /// INCL004: the search ran into something it cannot read, so it cannot say whether the property is loaded.
    /// The member that stopped it travels with the diagnostic, so that the code fix can offer to declare it.
    /// </summary>
    private static Diagnostic CannotCheck(
        Location location,
        IOperation? unreadableValue,
        IOperation value,
        string property)
        => Diagnostic.Create(
            NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                NavigationIncludeRulesProvider.Incl4IdCannotCheckNavigationProperty),
            location,
            UnreadableMember.GetProperties(unreadableValue),
            UnreadableMember.Describe(unreadableValue),
            value.Type?.Name,
            property,
            value.Syntax.ToString());

    /// <summary>
    /// The rule to report when the argument does not have the property loaded. Null for an argument nobody can
    /// annotate: a field, "new User()", the result of an arbitrary call.
    /// </summary>
    private static string? GetRuleForArgument(IOperation argument, FlowGraph flowGraph)
        => argument switch
        {
            // A local: the .Include() belongs in this method, so point at the argument.
            ILocalReferenceOperation
                => NavigationIncludeRulesProvider.Incl2IdArgumentDoesntIncludeNavigationProperty,

            // A parameter of this method: the Include is out of its scope, ask for [IncludeRequired].
            IParameterReferenceOperation reference when IsMethodParameter(reference.Parameter)
                => NavigationIncludeRulesProvider.Incl1IdAddIncludeRequiredForParameter,

            // "u" of "users.Select(u => ...)": the .Include() belongs on users.
            IParameterReferenceOperation reference when IsLinqLambdaParameter(reference.Parameter, flowGraph)
                => NavigationIncludeRulesProvider.Incl2IdArgumentDoesntIncludeNavigationProperty,

            _ => null,
        };

    private static bool IsMethodParameter(IParameterSymbol parameterSymbol)
        => parameterSymbol is
        {
            ContainingSymbol: IMethodSymbol
            {
                MethodKind: not MethodKind.AnonymousFunction
            }
        };

    private static bool IsLinqLambdaParameter(
        IParameterSymbol lambdaParameter,
        FlowGraph methodFlowGraph)
    {
        var lambdaInMethod = methodFlowGraph.FindLambda(lambdaParameter.ContainingSymbol);
        if (lambdaInMethod is null)
        {
            return false;
        }

        return BridgeCrosser.FromLambdaParameter(
            lambdaInMethod,
            lambdaParameter.Ordinal,
            methodFlowGraph.Bridges) is not null;
    }

    /// <summary>
    /// Returns the block that belongs to the method or constructor body.
    /// Other blocks (attribute arguments, parameter default values) have no control flow to analyze.
    /// </summary>
    private static IOperation? FindMethodBody(OperationBlockAnalysisContext context)
        => context.OperationBlocks.FirstOrDefault(block =>
            GetRoot(block) is IMethodBodyOperation or IConstructorBodyOperation);

    private static IOperation GetRoot(IOperation operation) =>
        operation.Parent is null
            ? operation
            : GetRoot(operation.Parent);
}
