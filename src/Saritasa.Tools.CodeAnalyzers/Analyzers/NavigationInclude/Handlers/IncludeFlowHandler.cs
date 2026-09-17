using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Entities;
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

        var diagnostics = new MethodFlowGraph(context.GetControlFlowGraph(body), method)
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
        var methodCalls = statementPosition.Statement
            .DescendantsAndSelf()
            .OfType<IInvocationOperation>();

        var diagnostics = Enumerable.Empty<Diagnostic>();

        foreach (var call in methodCalls)
        {
            var methodIncludeRequirements = AttributeHelper.GetIncludeRequirements(call.TargetMethod);

            var methodCallDiagnostics = ValidateIncludeRequirements(
                call,
                statementPosition,
                methodIncludeRequirements);

            diagnostics = diagnostics.Union(methodCallDiagnostics);
        }

        return diagnostics;
    }

    private static IEnumerable<Diagnostic> ValidateIncludeRequirements(
        IInvocationOperation call,
        CodePosition callStatement,
        IEnumerable<IncludeRequirement> methodIncludeRequirements)
    {
        foreach (var includeRequirement in methodIncludeRequirements)
        {
            var argument = call.Arguments.FirstOrDefault(argument =>
            {
                var parameterName = argument.Parameter?.Name;
                return parameterName == includeRequirement.ParameterName;
            });

            if (argument is null)
            {
                // Argument is not passed yet for this parameter.
                continue;
            }

            // "Process(user)" with a parameter of a base type, interface or nullable type wraps the local into
            // a conversion. ChooseRule matches the local itself, and the diagnostic reports its own type.
            var unwrappedValue = RoslynHelper.SkipWrappers(argument.Value);

            var ruleId = MapArgumentToDiagnostic(
                unwrappedValue,
                includeRequirement.NavigationProperty,
                callStatement);

            if (ruleId is null)
            {
                continue;
            }

            yield return Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(ruleId),
                call.Syntax.GetLocation(),
                unwrappedValue.Type?.Name,
                includeRequirement.NavigationProperty,
                unwrappedValue.Syntax.ToString());
        }
    }

    /// <summary>
    /// INCL003: for each [Includes(property)] of the method, the returned value must have the property loaded,
    /// unless the attribute is declared with <c>Verify = false</c>.
    /// </summary>
    private static IEnumerable<Diagnostic> GetReturnDiagnostics(CodePosition position)
    {
        if (!position.IsReturnFromMethod)
        {
            yield break;
        }

        var returnedValue = position.Statement;

        foreach (var property in AttributeHelper.GetNotVerifiedIncludes(position.FlowGraph.Method))
        {
            if (LoadedPropertySearch.IsLoaded(returnedValue, property, position))
            {
                continue;
            }

            // The graph keeps only the returned expression; report on the whole "return ...;" statement.
            var location = (returnedValue.Syntax.Parent as ReturnStatementSyntax ?? returnedValue.Syntax).GetLocation();

            yield return Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                    NavigationIncludeRulesProvider.Incl3IdMethodResultDoesntIncludeNavigationProperty),
                location,
                property);
        }
    }

    private static string? MapArgumentToDiagnostic(
        IOperation methodArgument,
        string navigationPropertyName,
        CodePosition methodCallStatement)
        => methodArgument switch
        {
            // Argument value is local reference. Try detect .Include().
            ILocalReferenceOperation when LoadedPropertySearch.IsLoaded(methodArgument, navigationPropertyName, methodCallStatement)
                => NavigationIncludeRulesProvider.Incl2IdArgumentDoesntIncludeNavigationProperty,

            // Argument value is parameter of parent method. Include is out of scope of this method. Ask to add [IncludeRequired]
            IParameterReferenceOperation parameterReference
                when IsMethodParameter(parameterReference.Parameter)
                => NavigationIncludeRulesProvider.Incl1IdAddIncludeRequiredForParameter,

            // Argument value is lambda parameter (u of users.Select(u => ...)). Try detect .Include().
            IParameterReferenceOperation referenceToLambdaParameter when
                IsLinqLambdaParameter(referenceToLambdaParameter.Parameter, methodCallStatement.FlowGraph) &&
                LoadedPropertySearch.IsLoaded(methodArgument, navigationPropertyName, methodCallStatement)
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
        IFlowGraph methodFlowGraph)
    {
        var lambdaInMethod = methodFlowGraph.FindLambda(lambdaParameter.ContainingSymbol);
        if (lambdaInMethod is null)
        {
            return false;
        }

        return LinqMethods.GetContainer(lambdaInMethod) is not null;
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
