using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// A move from one value to another that keeps the same entities: <c>query.Where(...)</c>,
/// <c>users.ToList()</c>, <c>users[0]</c>, <c>page.Items</c>, <c>query.Paginate(1)</c>.
/// </summary>
/// <remarks>
/// The search asks one thing here: may the entities have come out of this member, and out of which value?
/// The shape of the move does not matter. A collection can become another collection (<c>Where</c>), a
/// collection can become one element (<c>First</c>, <c>users[0]</c>), and one value can become a collection
/// (<c>page.Items</c>). What matters is that the entities on both sides are the same ones.
/// The move is permitted by a [PreservesIncludes] declaration, written on the member, on an assembly or built
/// in; nothing is guessed. What a declaration cannot say is read from the member's own declaration, see
/// <see cref="KeepsEntities"/>.
/// </remarks>
internal static class Transformation
{
    /// <summary>
    /// The value the entities came from, or null when the move out of this value is not permitted or there is
    /// nothing on the other side of it.
    /// </summary>
    /// <param name="value">Value the search is reading.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FindSource(Value value)
        => value.Operation switch
        {
            // "query.Where(...)", "users.ToList()", "query.Paginate(1)". Returns query/users
            IInvocationOperation call
                when FindDeclaredParameter(call.TargetMethod, value.Position) is { } parameterName
                => GetCallSource(call, parameterName),

            // "users[0]", "enumerator.Current", "pair.Value", "page.Items": the entities come from the object
            // the property is read on.
            IPropertyReferenceOperation reference
                when FindDeclaredParameter(reference.Property, value.Position) is not null
                => reference.Instance,

            // "users[0]" of an array. Roslyn has no member here, so no declaration can describe it, and there
            // is only one value the element can come from.
            IArrayElementReferenceOperation element
                => element.ArrayReference,

            _ => null,
        };

    /// <summary>
    /// The declared parameter the entities come from, and null when the move is not permitted: nobody declared
    /// the member, or this overload cannot hand the entities back. An empty name means the value the member is
    /// used on.
    /// </summary>
    private static string? FindDeclaredParameter(ISymbol member, CodePosition position)
    {
        var parameterName = position.FlowGraph.Declarations.FindSourceParameter(member);

        // A declaration names a member, not one overload: "ToDictionary(u => u.Id, u => u.Name)" is declared
        // together with "ToDictionary(u => u.Id)", but it hands back names, not users.
        return parameterName is not null && KeepsEntities(member, parameterName) ? parameterName : null;
    }

    /// <summary>
    /// The value a call hands its entities back from.
    /// </summary>
    /// <remarks>
    /// [PreservesIncludes] with no argument: the entities come from whatever the method is called on, e.g.
    /// "query" in "query.Paginate(1)", the same way as for Where, ToList and the rest.
    /// [PreservesIncludes(nameof(query))]: they come from the argument passed for that parameter, e.g.
    /// "dbContext.Users" in "dbContext.Users.Paginate(1)".
    /// </remarks>
    private static IOperation? GetCallSource(IInvocationOperation call, string parameterName)
        => parameterName.Length == 0
            ? RoslynHelper.GetReceiver(call)
            : call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == parameterName)?.Value;

    /// <summary>
    /// True if this overload of a declared member really hands back the entities it was given.
    /// </summary>
    /// <remarks>
    /// A declaration names a member, not an overload, so when the result is built from type parameters, one
    /// of them has to come from the source: <c>ToDictionary(u =&gt; u.Id)</c> returns
    /// <c>Dictionary&lt;TKey, TSource&gt;</c> and passes, <c>ToDictionary(u =&gt; u.Id, u =&gt; u.Name)</c>
    /// returns <c>Dictionary&lt;TKey, TElement&gt;</c> and does not. A result of concrete types only, such as
    /// <c>PagedResult&lt;User&gt;</c>, has nothing to compare, so the declaration is trusted.
    /// </remarks>
    private static bool KeepsEntities(ISymbol member, string parameterName)
    {
        var definition = member.OriginalDefinition;
        var result = definition switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            _ => null,
        };

        if (result is null || GetSourceType(definition, parameterName) is not { } source)
        {
            return false;
        }

        var resultParameters = RoslynHelper.GetTypesInside(result, withInterfaces: false)
            .OfType<ITypeParameterSymbol>()
            .ToList();

        if (resultParameters.Count == 0)
        {
            return true;
        }

        var sourceTypes = RoslynHelper.GetTypesInside(source, withInterfaces: true);

        return resultParameters.Any(sourceTypes.Contains);
    }

    /// <summary>
    /// The declared type the entities come from: the named parameter, or the type the member is used on when
    /// the declaration names no parameter.
    /// </summary>
    private static ITypeSymbol? GetSourceType(ISymbol definition, string parameterName)
        => definition is IMethodSymbol method && parameterName.Length > 0
            ? method.Parameters.FirstOrDefault(parameter => parameter.Name == parameterName)?.Type
            : RoslynHelper.GetReceiverType(definition);
}
