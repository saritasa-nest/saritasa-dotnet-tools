using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Answers one question: "is the navigation property loaded in this value?".
/// </summary>
/// <remarks>
/// A value never says by itself whether the property is loaded, so the search traces it back to the
/// <see cref="Origin"/>s its entities come from, the same way a person reads code. When several paths lead to
/// the place (if/else), the property must be loaded on every path.
/// Where an origin comes from is answered by <see cref="ExpressionOrigin"/> and <see cref="VariableOrigin"/>;
/// this class only keeps asking until every path ends at <see cref="Origin.Loaded"/>.
/// </remarks>
internal sealed class LoadedPropertySearch
{
    private readonly string property;
    private readonly VariableOrigin variableOrigin;

    private LoadedPropertySearch(string property)
    {
        this.property = property;
        variableOrigin = new VariableOrigin(property);
    }

    /// <summary>
    /// Returns true if the navigation property is loaded in the value.
    /// </summary>
    /// <param name="value">Value, e.g. a call argument or a returned expression.</param>
    /// <param name="property">Navigation property name.</param>
    /// <param name="position">Position of the statement that contains the value.</param>
    /// <returns>True if the property is loaded on every path.</returns>
    public static bool IsLoaded(IOperation value, string property, CodePosition position)
        => new LoadedPropertySearch(property).IsPropertyLoaded(Origin.Create(value, position));

    /// <summary>
    /// The whole search: trace the origin back to the origins it comes from, and require every one of them
    /// to have the property loaded.
    /// </summary>
    private bool IsPropertyLoaded(Origin origin) => origin switch
    {
        _ when origin == Origin.Loaded => true,
        _ when origin == Origin.NotFound => false,
        _ => GetOrigins(origin).All(IsPropertyLoaded)
    };

    /// <summary>
    /// Where the entities of an origin come from.
    /// </summary>
    /// <remarks>
    /// Never empty: a rule that cannot follow the entities answers with <see cref="Origin.NotFound"/>.
    /// </remarks>
    private IEnumerable<Origin> GetOrigins(Origin origin)
    {
        // "user", "users", "out var user", "#1": reading a variable is the only expression whose origin depends
        // on what the search has already read, so it is the only one that has to be walked back.
        if (VariableOrigin.GetRelatedVariable(origin.Value) is { } variable)
        {
            return variableOrigin.GetOrigins(variable, origin.Position);
        }

        // Every other expression answers from itself alone.
        return [ExpressionOrigin.GetOrigin(origin, property)];
    }
}
