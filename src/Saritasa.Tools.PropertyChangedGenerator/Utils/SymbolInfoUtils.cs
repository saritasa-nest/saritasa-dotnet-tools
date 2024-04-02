using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.PropertyChangedGenerator.Utils;

/// <summary>
/// Utils for <see cref="SymbolInfo"/>.
/// </summary>
internal static class SymbolInfoUtils
{
    /// <summary>
    /// Tries to get the resolved attribute type symbol from a given <see cref="SymbolInfo"/> value.
    /// </summary>
    /// <param name="symbolInfo">The <see cref="SymbolInfo"/> value to check.</param>
    /// <param name="typeSymbol">The resulting attribute type symbol, if correctly resolved.</param>
    /// <returns>Whether <paramref name="symbolInfo"/> is resolved to a symbol.</returns>
    /// <remarks>
    /// This can be used to ensure users haven't eg. spelled names incorrectly or missed a using directive. Normally, code would just
    /// not compile if that was the case, but that doesn't apply for attributes using invalid targets. In that case, Roslyn will ignore
    /// any errors, meaning the generator has to validate the type symbols are correctly resolved on its own.
    /// <br/>
    /// https://github.com/CommunityToolkit/dotnet/blob/7b53ae23dfc6a7fb12d0fc058b89b6e948f48448/src/CommunityToolkit.Mvvm.SourceGenerators/ComponentModel/ObservablePropertyGenerator.Execute.cs#L211-L217
    /// </remarks>
    public static bool TryGetAttributeTypeSymbol(this SymbolInfo symbolInfo, out INamedTypeSymbol? typeSymbol)
    {
        ISymbol? attributeSymbol = symbolInfo.Symbol;

        // If no symbol is selected and there is a single candidate symbol, use that
        if (attributeSymbol is null && symbolInfo.CandidateSymbols[0] is { } candidateSymbol)
        {
            attributeSymbol = candidateSymbol;
        }

        // Extract the symbol from either the current one or the containing type
        if ((attributeSymbol as INamedTypeSymbol ?? attributeSymbol?.ContainingType) is not { } resultingSymbol)
        {
            typeSymbol = null;

            return false;
        }

        typeSymbol = resultingSymbol;

        return true;
    }
}
