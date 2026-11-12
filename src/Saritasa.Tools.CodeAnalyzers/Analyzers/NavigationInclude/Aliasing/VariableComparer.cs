using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// Compares variables and the origins of objects: symbols the way Roslyn compares them, anything else, such as a
/// compiler temporary or an operation, by <see cref="object.Equals(object)"/>.
/// </summary>
internal sealed class VariableComparer : IEqualityComparer<object>
{
    private VariableComparer()
    {
    }

    /// <summary>
    /// The only instance.
    /// </summary>
    public static VariableComparer Instance { get; } = new();

    /// <inheritdoc />
    public new bool Equals(object? first, object? second)
        => first is ISymbol firstSymbol && second is ISymbol secondSymbol
            ? SymbolEqualityComparer.Default.Equals(firstSymbol, secondSymbol)
            : object.Equals(first, second);

    /// <inheritdoc />
    public int GetHashCode(object value)
        => value is ISymbol symbol
            ? SymbolEqualityComparer.Default.GetHashCode(symbol)
            : value.GetHashCode();
}
