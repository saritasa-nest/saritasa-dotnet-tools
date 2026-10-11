using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Roslyn;

/// <summary>
/// Batch of functions which hides roslyn details.
/// </summary>
internal static class RoslynReader
{
    /// <summary>
    /// Removes conversion and delegate creation wrappers around an operation.
    /// </summary>
    /// <param name="operation">Operation.</param>
    /// <returns>Operation without the wrappers.</returns>
    public static IOperation SkipWrappers(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => SkipWrappers(conversion.Operand),
            IDelegateCreationOperation delegateCreation => SkipWrappers(delegateCreation.Target),
            _ => operation,
        };

    /// <summary>
    /// The value the method is called on: <c>Instance</c> for instance methods, the first argument for
    /// extension methods (<c>query.Where(...)</c> is <c>Queryable.Where(query, ...)</c>).
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Receiver or null.</returns>
    public static IOperation? GetReceiver(IInvocationOperation call)
    {
        if (call.Instance is not null)
        {
            return call.Instance;
        }

        if (call.TargetMethod.IsExtensionMethod && call.Arguments.Length > 0)
        {
            return call.Arguments[0].Value;
        }

        return null;
    }

    /// <summary>
    /// The properties a lambda reads from its parameter, from the parameter outwards: ["Settings", "Profile"] for
    /// <c>u =&gt; u.Settings.Profile</c>. Null for any other body, such as <c>u =&gt; u.Orders.Where(...)</c>.
    /// </summary>
    /// <remarks>
    /// Reads syntax, because a lambda converted to an expression tree is data rather than code, and the compiler does
    /// not lower it into a graph the analyzer could read.
    /// </remarks>
    /// <param name="syntax">Syntax of the lambda.</param>
    /// <returns>Property names or null.</returns>
    public static IReadOnlyList<string>? GetMemberChain(SyntaxNode syntax)
    {
        var parameter = syntax switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesized
                => parenthesized.ParameterList.Parameters[0],
            _ => null,
        };

        if (parameter is null || syntax is not LambdaExpressionSyntax { ExpressionBody: { } body })
        {
            return null;
        }

        var chain = new List<string>();

        while (body is MemberAccessExpressionSyntax memberAccess)
        {
            chain.Insert(0, memberAccess.Name.Identifier.ValueText);
            body = memberAccess.Expression;
        }

        // The chain must start at the parameter: "u => other.Profile" reads nothing from u.
        var startsAtParameter = body is IdentifierNameSyntax identifier &&
                                identifier.Identifier.ValueText == parameter.Identifier.ValueText;

        return startsAtParameter && chain.Count > 0 ? chain : null;
    }
}
