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
}
