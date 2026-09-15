using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// A table "variable → navigation properties loaded for it" at one point of a method.
/// Example: <c>user → [Profile]</c>, <c>users → []</c>.
/// </summary>
/// <remarks>
/// A variable is one of:
/// <list type="bullet">
/// <item><see cref="ILocalSymbol"/> – a local variable;</item>
/// <item><see cref="IParameterSymbol"/> – a method or lambda parameter;</item>
/// <item><see cref="Microsoft.CodeAnalysis.FlowAnalysis.CaptureId"/> – a temporary value the compiler creates
/// in the control flow graph (for object initializers, <c>?:</c>, <c>??</c>, the foreach enumerator).</item>
/// </list>
/// The table is immutable: every change returns a new table, so a table saved for one block
/// is never changed while another block is processed.
/// </remarks>
internal sealed class LoadedProperties
{
    /// <summary>
    /// Table where nothing is known to be loaded.
    /// </summary>
    public static readonly LoadedProperties Empty =
        new(ImmutableDictionary.Create<object, ImmutableHashSet<string>>(VariableComparer.Instance));

    /// <summary>
    /// Empty list of properties.
    /// </summary>
    public static readonly ImmutableHashSet<string> NoProperties = ImmutableHashSet<string>.Empty;

    private readonly ImmutableDictionary<object, ImmutableHashSet<string>> propertiesByVariable;

    private LoadedProperties(ImmutableDictionary<object, ImmutableHashSet<string>> propertiesByVariable)
    {
        this.propertiesByVariable = propertiesByVariable;
    }

    /// <summary>
    /// Returns true if the table has a row for the variable.
    /// </summary>
    /// <param name="variable">Variable.</param>
    /// <returns>True if the variable is tracked.</returns>
    public bool IsTracked(object variable) => propertiesByVariable.ContainsKey(variable);

    /// <summary>
    /// Returns the properties loaded for the variable; empty if the variable is unknown.
    /// </summary>
    /// <param name="variable">Variable.</param>
    /// <returns>Loaded navigation property names.</returns>
    public ImmutableHashSet<string> GetProperties(object variable)
    {
        return propertiesByVariable.TryGetValue(variable, out var properties) ? properties : NoProperties;
    }

    /// <summary>
    /// Replaces the row for the variable (used for <c>variable = expression</c>).
    /// </summary>
    /// <param name="variable">Variable.</param>
    /// <param name="properties">Loaded navigation property names.</param>
    /// <returns>New table.</returns>
    public LoadedProperties SetProperties(object variable, ImmutableHashSet<string> properties)
    {
        return new LoadedProperties(propertiesByVariable.SetItem(variable, properties));
    }

    /// <summary>
    /// Adds one property to the row for the variable (used for <c>variable.Property = expression</c>).
    /// </summary>
    /// <param name="variable">Variable.</param>
    /// <param name="property">Navigation property name.</param>
    /// <returns>New table.</returns>
    public LoadedProperties AddProperty(object variable, string property)
    {
        return SetProperties(variable, GetProperties(variable).Add(property));
    }

    /// <summary>
    /// Merges tables of several paths that meet at the same point (e.g. after if/else):
    /// a property stays loaded only if it is loaded on every path.
    /// </summary>
    /// <param name="tables">Tables at the end of every incoming path.</param>
    /// <returns>Merged table; empty if there are no tables.</returns>
    public static LoadedProperties KeepOnlyWhatAllTablesAgreeOn(IReadOnlyList<LoadedProperties> tables)
    {
        if (tables.Count == 0)
        {
            return Empty;
        }

        var result = tables[0];
        for (var i = 1; i < tables.Count; i++)
        {
            result = KeepOnlyWhatBothTablesAgreeOn(result, tables[i]);
        }

        return result;
    }

    private static LoadedProperties KeepOnlyWhatBothTablesAgreeOn(LoadedProperties first, LoadedProperties second)
    {
        var result = Empty.propertiesByVariable.ToBuilder();
        foreach (var row in first.propertiesByVariable)
        {
            // A variable missing in one table means "nothing loaded", so it is dropped.
            if (second.propertiesByVariable.TryGetValue(row.Key, out var otherProperties))
            {
                result[row.Key] = row.Value.Intersect(otherProperties);
            }
        }

        return new LoadedProperties(result.ToImmutable());
    }

    /// <summary>
    /// Compares symbols with <see cref="SymbolEqualityComparer"/> and capture ids with Equals.
    /// </summary>
    private sealed class VariableComparer : IEqualityComparer<object>
    {
        public static readonly VariableComparer Instance = new();

        public new bool Equals(object? x, object? y)
        {
            if (x is ISymbol symbolX && y is ISymbol symbolY)
            {
                return SymbolEqualityComparer.Default.Equals(symbolX, symbolY);
            }

            return object.Equals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return obj is ISymbol symbol ? SymbolEqualityComparer.Default.GetHashCode(symbol) : obj.GetHashCode();
        }
    }
}
