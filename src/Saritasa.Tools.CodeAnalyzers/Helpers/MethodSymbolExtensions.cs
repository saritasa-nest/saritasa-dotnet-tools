using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Helpers;

/// <summary>
/// Extension methods for <see cref="IMethodSymbol"/>.
/// </summary>
public static class MethodSymbolExtensions
{
    /// <summary>
    /// Determines whether the specified method symbol represents <see cref="string.Format(string, object[])"/>.
    /// </summary>
    /// <param name="method">The method symbol to check.</param>
    /// <returns><c>true</c> if the method is <c>string.Format</c>; otherwise, <c>false</c>.</returns>
    public static bool IsStringFormat(this IMethodSymbol method)
    {
        return method.ContainingType.SpecialType == SpecialType.System_String && method.Name == "Format";
    }
}
