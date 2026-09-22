using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.EntityFramework;

/// <summary>
/// The one piece of Entity Framework knowledge the analyzer needs.
/// </summary>
/// <remarks>
/// Every other rule answers "do the entities pass through?". This one answers "which property was added?",
/// which no general rule can do: it has to know EF's <c>Include</c>. It lives alone so nobody mistakes it for
/// a general rule.
/// </remarks>
internal static class EfIncludeReader
{
    private const string QueryableExtensions = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";

    /// <summary>
    /// Returns "Profile" for <c>Include(u =&gt; u.Profile)</c> or <c>Include("Profile")</c>;
    /// null for other calls. Filtered includes such as <c>u =&gt; u.Orders.Where(...)</c> are not recognized.
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Property name or null.</returns>
    public static string? GetIncludedProperty(IInvocationOperation call)
    {
        var method = call.TargetMethod;
        if (method.Name != "Include" ||
            method.ContainingType.ToDisplayString() != QueryableExtensions ||
            call.Arguments.Length < 2)
        {
            return null;
        }

        var argument = call.Arguments[1].Value;
        if (argument.ConstantValue is { HasValue: true, Value: string path })
        {
            return path;
        }

        // "Include" takes an expression tree, which is data rather than code, so the compiler does not lower it
        // into a graph the analyzer could read. The property name is taken from the syntax "u => u.Profile".
        if (argument.Syntax is LambdaExpressionSyntax { ExpressionBody: MemberAccessExpressionSyntax memberAccess })
        {
            return memberAccess.Name.Identifier.ValueText;
        }

        return null;
    }
}
