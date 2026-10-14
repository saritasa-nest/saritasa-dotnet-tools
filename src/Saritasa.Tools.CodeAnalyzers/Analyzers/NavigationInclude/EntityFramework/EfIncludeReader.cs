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
    /// Returns the path the call loads:
    /// <list type="bullet">
    /// <item>"Profile" for <c>Include(u =&gt; u.Profile)</c> or <c>Include("Profile")</c>;</item>
    /// <item>"Settings.Profile" for <c>Include(u =&gt; u.Settings.Profile)</c> or <c>Include("Settings.Profile")</c>;</item>
    /// <item>"Settings.Profile" for <c>Include(u =&gt; u.Settings).ThenInclude(s =&gt; s.Profile)</c>;</item>
    /// <item>"Orders" for the filtered include <c>Include(u =&gt; u.Orders.Where(...).Take(5))</c>, which loads some
    /// of the orders but fills the collection.</item>
    /// </list>
    /// Null for other calls.
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Included path or null.</returns>
    public static NavigationPath? GetIncludedPath(IInvocationOperation call)
    {
        var method = call.TargetMethod;
        if (method.ContainingType.ToDisplayString() != QueryableExtensions || call.Arguments.Length < 2)
        {
            return null;
        }

        return method.Name switch
        {
            "Include" => ReadIncludeArgument(call.Arguments[1].Value),
            "ThenInclude" => ReadThenInclude(call),
            _ => null,
        };
    }

    /// <summary>
    /// "Include(u =&gt; u.Settings.Profile)" or "Include("Settings.Profile")".
    /// </summary>
    private static NavigationPath? ReadIncludeArgument(IOperation argument)
    {
        if (argument.ConstantValue is { HasValue: true, Value: string path })
        {
            return NavigationPath.Parse([path]);
        }

        return ReadLambdaChain(argument.Syntax) is { } chain
            ? NavigationPath.Parse(chain)
            : null;
    }

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

    /// <summary>
    /// "query.Include(u =&gt; u.Settings).ThenInclude(s =&gt; s.Profile)": the path of the Include or ThenInclude it is
    /// called on, followed by its own lambda.
    /// </summary>
    private static NavigationPath? ReadThenInclude(IInvocationOperation call)
    {
        // "s => s.Profile": what this ThenInclude adds.
        var ownChain = ReadLambdaChain(call.Arguments[1].Value.Syntax);
        if (ownChain is null)
        {
            return null;
        }

        var receiver = RoslynReader.GetReceiver(call);
        var previousCall = receiver is null
            ? null
            : RoslynReader.SkipWrappers(receiver) as IInvocationOperation;
        if (previousCall is null)
        {
            return null;
        }

        // "Include(u => u.Settings)": the path this ThenInclude continues.
        var previousPath = GetIncludedPath(previousCall);
        if (previousPath is null)
        {
            return null;
        }

        return NavigationPath.Parse(previousPath.Segments.Concat(ownChain));
    }
}
