using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Where the entities of an expression come from: a call, an indexer, a property that returns elements.
/// </summary>
/// <remarks>
/// The counterpart of <see cref="VariableOrigin"/>. Reading a variable is an expression too, but it is the one
/// case that cannot be answered here: a variable has to be traced back through the statements that ran before
/// it, which needs the search and its memory of the blocks already read. Every other expression answers from
/// itself alone and never moves the position.
/// To support a new way of getting entities, add a rule to <see cref="GetOrigin"/>.
/// </remarks>
internal static class ExpressionOrigin
{
    /// <summary>
    /// Returns where the entities of the expression come from.
    /// </summary>
    /// <param name="origin">Origin whose value is read.</param>
    /// <param name="property">Navigation property the search is looking for.</param>
    /// <returns>Origin; <see cref="Origin.NotFound"/> when the expression cannot be followed.</returns>
    public static Origin GetOrigin(Origin origin, string property)
        => origin.Value switch
        {
            // "query.Include(u => u.Profile)", a call of a method declared with [Includes("Profile")].
            IInvocationOperation call when LoadsProperty(call, property)
                => Origin.Loaded,

            // "query.Where(...)", "users.ToList()": the entities are the ones of the value the call is made on.
            IInvocationOperation call
                => Origin.Create(GetEntitiesSource(call), origin.Position),

            // "users[0]", "enumerator.Current", "dictionary.Values": the entities are elements of the collection.
            IPropertyReferenceOperation reference when ReturnsElements(reference.Property)
                => Origin.Create(reference.Instance, origin.Position),

            // "dbContext.Users", "new User()", a field.
            _ => Origin.NotFound,
        };

    /// <summary>
    /// True if the call loads the property itself: <c>query.Include(u =&gt; u.Profile)</c> or a call of a method
    /// declared with <c>[Includes("Profile")]</c>.
    /// </summary>
    private static bool LoadsProperty(IInvocationOperation call, string property)
        => AttributeHelper.MethodHasIncludesAttribute(call.TargetMethod, property) ||
           LinqMethods.GetIncludedProperty(call) == property;

    /// <summary>
    /// Maps a call to the value whose entities it returns: <c>query</c> for <c>query.Where(...)</c>. Null if the
    /// call returns other objects (<c>Select</c>, arbitrary methods).
    /// </summary>
    private static IOperation? GetEntitiesSource(IInvocationOperation call)
        => call.TargetMethod.Name switch
        {
            // foreach is rewritten by the compiler to "enumerator = collection.GetEnumerator()".
            "GetEnumerator" or "GetAsyncEnumerator" => call.Instance,

            // dictionary.GetValueOrDefault(id): an extension method, the dictionary is the first argument.
            "GetValueOrDefault" => LinqMethods.GetSource(call),

            _ when LinqMethods.KeepsSourceEntities(call.TargetMethod) => LinqMethods.GetSource(call),
            _ => null,
        };

    /// <summary>
    /// Properties returning elements of the collection they are called on:
    /// "item = enumerator.Current" (foreach is rewritten by the compiler to it), users[0], dictionary[id],
    /// dictionary.Values, pair.Value of a dictionary entry.
    /// </summary>
    private static bool ReturnsElements(IPropertySymbol property)
        => property.IsIndexer ||
           property.Name is "Current" or "Values" ||
           (property.Name == "Value" && property.ContainingType.Name == "KeyValuePair");
}
