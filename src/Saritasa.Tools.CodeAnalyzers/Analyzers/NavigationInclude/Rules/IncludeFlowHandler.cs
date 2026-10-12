using Microsoft.CodeAnalysis.CSharp;
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
/// Finds every read of a [TrackIncludeRequired] property, every call of an [IncludeRequired] method and every
/// return of an [Includes] method, and asks <see cref="IncludeSearcher"/> whether the property is loaded there.
/// Reports INCL001, INCL002, INCL003 and INCL004.
/// </summary>
/// <remarks>
/// All three are the same question, "is the path loaded in this value?", asked of different values: the value in
/// front of a read, an argument, a returned value. The answer says where the search ended, and that decides the
/// diagnostic.
/// </remarks>
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

        var statements = new FlowGraph(context.GetControlFlowGraph(body), method, bridges)
            .GetStatements(context.CancellationToken)
            .Where(position => position.Statement is not null);

        foreach (var position in statements)
        {
            var statement = position.Statement;
            foreach (var operation in statement.DescendantsAndSelf())
            {
                switch (operation)
                {
                    case IPropertyReferenceOperation read when TryCheckRead(read, position, out var readDiagnostic):
                        context.ReportDiagnostic(readDiagnostic);
                        break;

                    case IInvocationOperation call when TryCheckCall(call, position, out var callDiagnostics):
                        ReportDiagnostics(context, callDiagnostics);
                        break;
                }
            }

            // A return is a statement, not an operation inside one.
            if (TryCheckReturn(position, out var returnDiagnostics))
            {
                ReportDiagnostics(context, returnDiagnostics);
            }
        }
    }

    private static void ReportDiagnostics(OperationBlockAnalysisContext context, List<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// INCL001 / INCL002 / INCL004: a read of a [TrackIncludeRequired] property must have it loaded. Every read is
    /// checked on its own: "Profile" of "user.Settings.Profile" asks for "Settings.Profile" of user, and
    /// "Settings" asks for "Settings".
    /// </summary>
    /// <returns>True if the read has to be reported.</returns>
    private static bool TryCheckRead(
        IPropertyReferenceOperation read,
        CodePosition position,
        out Diagnostic diagnostic)
    {
        // Only read when the method returns true.
        diagnostic = null!;

        // Only [TrackIncludeRequired] properties are tracked.
        if (!AttributeReader.HasTrackIncludeRequiredAttribute(read.Property))
        {
            return false;
        }

        // "this.Profile" inside the entity has no query behind it.
        if (read.Instance is null or IInstanceReferenceOperation)
        {
            return false;
        }

        // "user.Profile = x" and object initializers write the property, they do not read it.
        if (IsAssignmentTarget(read.Syntax))
        {
            return false;
        }

        // A lambda that becomes an expression tree is translated, never run.
        if (position.FlowGraph.IsExpressionTree)
        {
            return false;
        }

        var path = NavigationPath.Parse([read.Property.Name])!;
        var answer = IncludeSearcher.Check(read.Instance, path, position);

        if (GetDiagnostic(answer, GetPropertyNameLocation(read), read.Instance, path) is not { } found)
        {
            return false;
        }

        diagnostic = found;
        return true;
    }

    /// <summary>
    /// INCL001 / INCL002 / INCL004: for each [IncludeRequired(parameter, path)] of the called method, the passed
    /// value must have the path loaded.
    /// </summary>
    /// <returns>True if the call has anything to report.</returns>
    private static bool TryCheckCall(
        IInvocationOperation call,
        CodePosition position,
        out List<Diagnostic> diagnostics)
    {
        diagnostics = [];

        foreach (var requirement in AttributeReader.GetIncludeRequirements(call.TargetMethod))
        {
            var argument = call.Arguments.FirstOrDefault(argument =>
                argument.Parameter?.Name == requirement.ParameterName);

            // Argument is not passed yet for this parameter.
            if (argument is null)
            {
                continue;
            }

            var value = RoslynReader.SkipWrappers(argument.Value);
            var answer = IncludeSearcher.Check(value, requirement.Path, position);
            var diagnostic = GetDiagnostic(answer, call.Syntax.GetLocation(), value, requirement.Path);

            // The same requirement annotated twice is still one problem.
            if (diagnostic is not null && !diagnostics.Contains(diagnostic))
            {
                diagnostics.Add(diagnostic);
            }
        }

        return diagnostics.Count > 0;
    }

    /// <summary>
    /// INCL003 / INCL004: for each [Includes(path)] of the method, the returned value must have the path loaded,
    /// unless the attribute sets <c>Verify = false</c>.
    /// </summary>
    /// <returns>True if the return has anything to report.</returns>
    private static bool TryCheckReturn(CodePosition position, out List<Diagnostic> diagnostics)
    {
        diagnostics = [];

        if (!position.IsReturnFromMethod || position.Statement is not { } returnedValue)
        {
            return false;
        }

        // The graph keeps only the returned expression; report on the whole "return ...;" statement.
        var location = (returnedValue.Syntax.Parent as ReturnStatementSyntax ?? returnedValue.Syntax).GetLocation();

        foreach (var path in AttributeReader.GetIncludesToVerify(position.FlowGraph.Method))
        {
            var answer = IncludeSearcher.Check(returnedValue, path, position);

            // The method itself promised the path, so wherever the search ended, the promise is broken here.
            // Loaded and unknown answers mean the same as anywhere else.
            var diagnostic = answer is Answer.NotLoadedAnswer or Answer.NotRequiredByAnswer
                ? Diagnostic.Create(
                    NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                        NavigationIncludeRulesProvider.Incl3IdMethodResultDoesntIncludeNavigationProperty),
                    location,
                    path.ToString(),
                    GetQueryPath(answer, path).ToString())
                : GetDiagnostic(answer, location, returnedValue, path);

            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
        }

        return diagnostics.Count > 0;
    }

    /// <summary>
    /// The path the query has to Include: counted from where the search ended when it ended at a query, otherwise
    /// the promised path itself.
    /// </summary>
    private static NavigationPath GetQueryPath(Answer answer, NavigationPath promisedPath)
        => answer is Answer.NotLoadedAnswer missing
            ? missing.Path
            : promisedPath;

    /// <summary>
    /// The diagnostic for a value that must have the path loaded, chosen by where the search ended: INCL001 at a
    /// parameter of a method that does not ask for the path, INCL002 at a query or a method that does not load
    /// it, INCL004 where the search could not read further. Null when the path is loaded.
    /// </summary>
    private static Diagnostic? GetDiagnostic(Answer answer, Location location, IOperation value, NavigationPath path)
        => answer switch
        {
            Answer.LoadedAnswer => null,

            // The search could not read the whole path, so report that instead of a mistake in the code. The member
            // that stopped it travels with the diagnostic, so that the code fix can offer to annotate it.
            Answer.UnknownAnswer unknown => Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                    NavigationIncludeRulesProvider.Incl4IdCannotCheckNavigationProperty),
                location,
                UnreadableMember.GetProperties(unknown.UnreadableValue),
                UnreadableMember.Describe(unknown.UnreadableValue),
                value.Type?.Name,
                path.ToString(),
                value.Syntax.ToString()),

            // The Include is out of the method's scope: ask the caller for the whole path with [IncludeRequired].
            Answer.NotRequiredByAnswer missing => Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                    NavigationIncludeRulesProvider.Incl1IdAddIncludeRequiredForParameter),
                location,
                missing.Parameter.Type.Name,
                missing.Path.ToString(),
                missing.Parameter.Name),

            // The entities came from a query or a method that does not load the path. The fix goes where the search
            // ended, so the path to load is counted from there.
            Answer.NotLoadedAnswer missing => Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                    NavigationIncludeRulesProvider.Incl2IdArgumentDoesntIncludeNavigationProperty),
                location,
                value.Type?.Name,
                path.ToString(),
                value.Syntax.ToString(),
                missing.Path.ToString()),

            _ => throw new ArgumentOutOfRangeException(nameof(answer), "Unknown answer."),
        };

    /// <summary>
    /// True for the left side of a plain "=". It is read on the code as written, since the graph may first
    /// capture the target and assign it later. "??=" reads the property, so it is not a plain "=".
    /// </summary>
    private static bool IsAssignmentTarget(SyntaxNode syntax)
        => syntax.Parent is AssignmentExpressionSyntax assignment &&
           assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
           assignment.Left == syntax;

    /// <summary>
    /// The name of the property: "Profile" of "user.Settings.Profile" or of "user?.Profile".
    /// </summary>
    private static Location GetPropertyNameLocation(IPropertyReferenceOperation reference)
        => reference.Syntax switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.GetLocation(),
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.GetLocation(),
            _ => reference.Syntax.GetLocation(),
        };

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
