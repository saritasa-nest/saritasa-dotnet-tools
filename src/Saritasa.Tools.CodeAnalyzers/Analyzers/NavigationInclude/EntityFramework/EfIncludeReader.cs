using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Roslyn;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.EntityFramework;

/// <summary>
/// The one piece of Entity Framework knowledge the analyzer needs.
/// </summary>
/// <remarks>
/// Every other rule answers "do the entities pass through?". This one answers "which path was added?",
/// which needs to know EF's <c>Include</c> and <c>ThenInclude</c>. It lives alone so nobody takes it for a general rule.
/// </remarks>
internal static class EfIncludeReader
{
    private const string QueryableExtensions = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";

    /// <summary>
    /// Returns the properties an Include loads, starting at the entity. Null for other calls.
    /// <list type="bullet">
    /// <item>["Settings", "Profile"] for <c>Include(u =&gt; u.Settings.Profile)</c> or
    /// <c>Include("Settings.Profile")</c>;</item>
    /// <item>["Orders"] for the filtered <c>Include(u =&gt; u.Orders.Where(...).Take(5))</c>, which loads some of the
    /// orders but fills the collection.</item>
    /// </list>
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Property names or null.</returns>
    public static IReadOnlyList<string>? ReadInclude(IInvocationOperation call)
    {
        if (!IsQueryableExtension(call, "Include"))
        {
            return null;
        }

        var argument = call.Arguments[1].Value;

        // "Include("Settings.Profile")".
        if (argument.ConstantValue is { HasValue: true, Value: string path })
        {
            return NavigationPath.Parse([path])?.Segments;
        }

        return ReadLambdaChain(argument.Syntax);
    }

    /// <summary>
    /// Returns the properties a ThenInclude adds where the chain it is called on ends: ["Profile"] for
    /// <c>ThenInclude(s =&gt; s.Profile)</c>, also through a filter. Null for other calls. Which chain that is, is
    /// the search's question.
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Property names or null.</returns>
    public static IReadOnlyList<string>? ReadThenInclude(IInvocationOperation call)
        => IsQueryableExtension(call, "ThenInclude")
            ? ReadLambdaChain(call.Arguments[1].Value.Syntax)
            : null;

    /// <summary>
    /// True for a call of the EF method with the navigation argument.
    /// </summary>
    private static bool IsQueryableExtension(IInvocationOperation call, string methodName)
        => call.TargetMethod.Name == methodName &&
           call.TargetMethod.ContainingType.ToDisplayString() == QueryableExtensions &&
           call.Arguments.Length >= 2;

    /// <summary>
    /// The properties an Include or ThenInclude lambda loads: ["Settings", "Profile"] for
    /// <c>u =&gt; u.Settings.Profile</c>, and ["Orders"] for the filtered <c>u =&gt; u.Orders.Where(...).Take(5)</c>,
    /// which loads some of the orders but fills the collection.
    /// </summary>
    private static IReadOnlyList<string>? ReadLambdaChain(SyntaxNode lambda)
    {
        var parameter = RoslynReader.GetLambdaParameter(lambda, position: 0);
        var body = RoslynReader.GetLambdaBody(lambda);

        return parameter is null || body is null
            ? null
            : RoslynReader.ReadMemberChain(RoslynReader.TrimCalls(body), parameter);
    }
}
