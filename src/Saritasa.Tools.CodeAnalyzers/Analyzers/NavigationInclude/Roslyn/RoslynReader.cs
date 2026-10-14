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
    /// True if the type is the one with the full metadata name, generic arguments aside: "DbSet&lt;User&gt;" is
    /// "Microsoft.EntityFrameworkCore.DbSet`1".
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="fullMetadataName">Namespace and metadata name, e.g. "System.Linq.Expressions.Expression`1".</param>
    /// <returns>True if the type has the name.</returns>
    public static bool IsType(ITypeSymbol? type, string fullMetadataName)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        var containingNamespace = namedType.ContainingNamespace;
        var fullName = containingNamespace is null || containingNamespace.IsGlobalNamespace
            ? namedType.MetadataName
            : containingNamespace.ToDisplayString() + "." + namedType.MetadataName;

        return string.Equals(fullName, fullMetadataName, StringComparison.Ordinal);
    }

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
    /// The parameter of a lambda at the position: <c>u</c> at 0 of <c>u =&gt; ...</c> or <c>(u, i) =&gt; ...</c>,
    /// <c>i</c> at 1. Null when there is no such parameter, or for anything that is not a lambda.
    /// </summary>
    /// <remarks>
    /// The functions on lambda syntax read the lambda as written, because a lambda converted to an expression tree is
    /// data rather than code, and the compiler does not lower it into a graph the analyzer could read.
    /// </remarks>
    /// <param name="lambda">Syntax of the lambda.</param>
    /// <param name="position">Position of the parameter, from 0.</param>
    /// <returns>Parameter or null.</returns>
    public static ParameterSyntax? GetLambdaParameter(SyntaxNode lambda, int position)
        => lambda switch
        {
            SimpleLambdaExpressionSyntax simple when position == 0 => simple.Parameter,
            ParenthesizedLambdaExpressionSyntax parenthesized
                when position < parenthesized.ParameterList.Parameters.Count
                => parenthesized.ParameterList.Parameters[position],
            _ => null,
        };

    /// <summary>
    /// The expression a lambda returns: <c>u.Settings.Profile</c> of <c>u =&gt; u.Settings.Profile</c>. Null for a
    /// lambda with a block body, or for anything that is not a lambda.
    /// </summary>
    /// <param name="lambda">Syntax of the lambda.</param>
    /// <returns>Body expression or null.</returns>
    public static ExpressionSyntax? GetLambdaBody(SyntaxNode lambda)
        => lambda is LambdaExpressionSyntax { ExpressionBody: { } body } ? body : null;

    /// <summary>
    /// Removes the method calls at the end of an expression: <c>u.Orders</c> of <c>u.Orders.Where(...).Take(5)</c>.
    /// </summary>
    /// <param name="expression">Expression.</param>
    /// <returns>The expression in front of the calls; the expression itself when it does not end in a call.</returns>
    public static ExpressionSyntax TrimCalls(ExpressionSyntax expression)
    {
        while (expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax calledMethod })
        {
            expression = calledMethod.Expression;
        }

        return expression;
    }

    /// <summary>
    /// The properties an expression reads from the parameter, from the parameter outwards: ["Settings", "Profile"]
    /// for <c>u.Settings.Profile</c>. Null when the expression is not a chain of member reads starting at the
    /// parameter, such as <c>other.Profile</c> or <c>u.Orders.Where(...)</c>.
    /// </summary>
    /// <param name="expression">Expression.</param>
    /// <param name="parameter">Parameter the chain must start at.</param>
    /// <returns>Property names or null.</returns>
    public static IReadOnlyList<string>? ReadMemberChain(ExpressionSyntax expression, ParameterSyntax parameter)
    {
        var chain = new List<string>();

        while (expression is MemberAccessExpressionSyntax memberAccess)
        {
            chain.Insert(0, memberAccess.Name.Identifier.ValueText);
            expression = memberAccess.Expression;
        }

        // The chain must start at the parameter: "u => other.Profile" reads nothing from u.
        var startsAtParameter = expression is IdentifierNameSyntax identifier &&
                                identifier.Identifier.ValueText == parameter.Identifier.ValueText;

        return startsAtParameter && chain.Count > 0 ? chain : null;
    }
}
