using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Batch of functions which hides roslyn details.
/// </summary>
internal static class RoslynHelper
{
    /// <summary>
    /// Removes conversion and delegate creation wrappers around an operation.
    /// </summary>
    /// <example>
    /// A lambda has no type of its own, so Roslyn wraps it in a node that turns it into a delegate or expression tree.
    /// For <c>Func&lt;User, Address&gt; f = u =&gt; u.Address;</c> the value is:
    /// <code>
    /// IDelegateCreationOperation           // skipped
    ///   └─ IAnonymousFunctionOperation     // returned: u => u.Address
    /// </code>
    /// For <c>(object)user</c> the value is:
    /// <code>
    /// IConversionOperation                 // skipped
    ///   └─ IParameterReferenceOperation    // returned: user
    /// </code>
    /// Wrappers can be nested, so they are skipped until a different operation is found.
    /// </example>
    /// <param name="operation">Operation.</param>
    /// <returns>Operation without wrappers.</returns>
    public static IOperation SkipWrappers(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => SkipWrappers(conversion.Operand),
            IDelegateCreationOperation delegateCreation => SkipWrappers(delegateCreation.Target),
            _ => operation,
        };
}
