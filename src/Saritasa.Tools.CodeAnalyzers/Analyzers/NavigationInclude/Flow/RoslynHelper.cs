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
