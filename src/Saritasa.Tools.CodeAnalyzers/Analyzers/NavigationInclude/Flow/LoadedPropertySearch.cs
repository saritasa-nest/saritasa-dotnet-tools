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

    /// <summary>
    /// The value the search could not follow, kept so that a report can name it. The first one set while the
    /// recursion unwinds is the deepest, which is the one that actually stopped the search.
    /// </summary>
    private IOperation? unknownSource;

    private LoadedPropertySearch(string property)
    {
        this.property = property;
        variableOrigin = new VariableOrigin(property);
    }

    /// <summary>
    /// Answers whether the navigation property is loaded in the value.
    /// </summary>
    /// <param name="value">Value, e.g. a call argument or a returned expression.</param>
    /// <param name="property">Navigation property name.</param>
    /// <param name="position">Position of the statement that contains the value.</param>
    /// <param name="unknownSource">
    /// The value the search could not follow, when the answer is <see cref="Origin.Unknown"/>. Null otherwise.
    /// </param>
    /// <returns>
    /// <see cref="Origin.Loaded"/> when the property is loaded on every path, <see cref="Origin.Missing"/>
    /// when a path was followed to the end without it, and <see cref="Origin.Unknown"/> when a path ran into
    /// something the search cannot read.
    /// </returns>
    public static Origin Check(
        IOperation value,
        string property,
        CodePosition position,
        out IOperation? unknownSource)
    {
        var search = new LoadedPropertySearch(property);
        var answer = search.CheckOrigin(Origin.Create(value, position));

        unknownSource = search.unknownSource;

        return answer;
    }

    /// <summary>
    /// The whole search: trace the origin back to the origins it comes from, and require every one of them
    /// to have the property loaded.
    /// </summary>
    /// <remarks>
    /// The first origin that is not loaded is the answer, so the search reads no more of the code than it has
    /// to. That also means a path it cannot read hides a later path it could have answered, which is the safe
    /// way round: a suggestion is shown instead of a warning.
    /// </remarks>
    private Origin CheckOrigin(Origin origin)
    {
        if (origin == Origin.Loaded || origin == Origin.Missing || origin == Origin.Unknown)
        {
            return origin;
        }

        foreach (var next in GetOrigins(origin))
        {
            var answer = CheckOrigin(next);
            if (answer != Origin.Loaded)
            {
                if (answer == Origin.Unknown)
                {
                    // This origin is the one whose value could not be followed any further.
                    unknownSource ??= origin.Value;
                }

                return answer;
            }
        }

        return Origin.Loaded;
    }

    /// <summary>
    /// Where the entities of an origin come from.
    /// </summary>
    /// <remarks>
    /// Never empty: a rule that cannot follow the entities answers with an origin that ends the search.
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
