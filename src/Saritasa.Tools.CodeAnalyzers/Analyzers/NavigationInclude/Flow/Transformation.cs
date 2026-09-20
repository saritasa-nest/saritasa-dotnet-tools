using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// A member that hands back the entities it was given: <c>query.Where(...)</c>, <c>users.ToList()</c>,
/// <c>users[0]</c>, <c>page.Items</c>, <c>query.Paginate(1)</c>.
/// </summary>
/// <remarks>
/// A transformation makes a new value out of another one and keeps the same entities inside, so the search
/// leaves it and goes on with the value it was made from. This class answers the three questions about that,
/// and it is the only place that reads declarations:
/// where a call hands its entities back from, whether a property passes them on, and which collection a lambda
/// receives its elements from.
/// Which members pass entities on is never guessed: it comes from a [PreservesIncludes] declaration, written on
/// the member, written on an assembly or built in. What a declaration cannot say is read from the member's own
/// declaration, see <see cref="HandsBackSource"/> and <see cref="ReceivesSourceElements"/>.
/// </remarks>
internal static class Transformation
{
    /// <summary>
    /// Returns the value a call hands its entities back from. Null when the call declares nothing, when this
    /// overload cannot hand the entities back, or when the source is not there to follow.
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <param name="position">Position the call is read at.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FindCallSource(IInvocationOperation call, CodePosition position)
    {
        var parameterName = position.FlowGraph.Declarations.FindSourceParameter(call.TargetMethod);

        // A declaration names a method, not one overload: "ToDictionary(u => u.Id, u => u.Name)" is declared
        // together with "ToDictionary(u => u.Id)", but it hands back names, not users.
        if (parameterName is null || !HandsBackSource(call.TargetMethod, parameterName))
        {
            return null;
        }

        // [PreservesIncludes] with no argument: entities come from whatever the method is called on, e.g.
        // "query" in "query.Paginate(1)" (GetSource finds it the same way for Where, ToList, ...).
        // [PreservesIncludes(nameof(query))]: entities come from the argument passed for that parameter, e.g.
        // "dbContext.Users" in "dbContext.Users.Paginate(1)".
        return parameterName.Length == 0
            ? GetSource(call)
            : call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == parameterName)?.Value;
    }

    /// <summary>
    /// True if the property hands back the entities of the object it is read on: <c>users[0]</c>,
    /// <c>enumerator.Current</c>, <c>pair.Value</c>, <c>page.Items</c>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="position">Position the property is read at.</param>
    /// <returns>True if the property passes entities on.</returns>
    public static bool PassesEntitiesOn(IPropertySymbol property, CodePosition position)
        => position.FlowGraph.Declarations.FindSourceParameter(property) is { } parameterName &&
           HandsBackSource(property, parameterName);

    /// <summary>
    /// For <c>users.Select(u =&gt; ...)</c> returns <c>users</c>: the collection whose elements the lambda
    /// receives. Null if the lambda receives something else.
    /// </summary>
    /// <param name="lambda">Lambda.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FindElementsSource(IFlowAnonymousFunctionOperation lambda)
    {
        var parent = lambda.Parent;
        while (parent is IConversionOperation or IDelegateCreationOperation)
        {
            parent = parent.Parent;
        }

        if (parent is IArgumentOperation { Parameter: { } lambdaParameter, Parent: IInvocationOperation call } &&
            ReceivesSourceElements(lambdaParameter, call.TargetMethod))
        {
            return GetSource(call);
        }

        return null;
    }

    /// <summary>
    /// Returns the collection or query the method is called on: <c>Instance</c> for instance methods,
    /// the first argument for extension methods (<c>query.Where(...)</c> is <c>Queryable.Where(query, ...)</c>).
    /// </summary>
    private static IOperation? GetSource(IInvocationOperation call)
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

    /// <summary>
    /// Returns true if this overload of a declared member really hands back the entities of its source.
    /// </summary>
    /// <remarks>
    /// A declaration names a member, not an overload, so it is checked against the member's own declaration:
    /// when the result is built from type parameters, one of them has to come from the source.
    /// <c>ToDictionary(u =&gt; u.Id)</c> returns <c>Dictionary&lt;TKey, TSource&gt;</c> and passes;
    /// <c>ToDictionary(u =&gt; u.Id, u =&gt; u.Name)</c> returns <c>Dictionary&lt;TKey, TElement&gt;</c> and
    /// does not. A result built from concrete types only, such as
    /// <c>PagedResult&lt;User&gt; Paginate(IQueryable&lt;User&gt;)</c>, has nothing to compare, so the
    /// declaration is trusted.
    /// </remarks>
    private static bool HandsBackSource(ISymbol member, string parameterName)
    {
        var definition = member.OriginalDefinition;
        var result = definition switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            _ => null,
        };

        if (result is null || GetDeclaredSource(definition, parameterName) is not { } source)
        {
            return false;
        }

        var resultParameters = GetTypesInside(result, withInterfaces: false)
            .OfType<ITypeParameterSymbol>()
            .ToList();

        if (resultParameters.Count == 0)
        {
            return true;
        }

        var sourceTypes = GetTypesInside(source, withInterfaces: true);

        return resultParameters.Any(sourceTypes.Contains);
    }

    /// <summary>
    /// Returns true if a lambda passed to this parameter receives elements of the source, and not something else.
    /// </summary>
    /// <remarks>
    /// This is not a question of where a result comes from, so no declaration can answer it; it is read from the
    /// method's own declaration instead. The selector of <c>Select</c> is declared <c>Func&lt;TSource, TResult&gt;</c>
    /// and receives elements, while the result selector of <c>GroupBy</c> is declared
    /// <c>Func&lt;TKey, IEnumerable&lt;TSource&gt;, TResult&gt;</c> and receives the key, which comparing the
    /// types at the call cannot tell apart when the key happens to be an entity as well.
    /// A project's own <c>ForEachItem(this IEnumerable&lt;T&gt;, Action&lt;T&gt;)</c> reads the same way.
    /// </remarks>
    private static bool ReceivesSourceElements(IParameterSymbol lambdaParameter, IMethodSymbol method)
    {
        var definition = method.OriginalDefinition;
        if (definition.Parameters.Length <= lambdaParameter.Ordinal ||
            GetDeclaredSource(definition, parameterName: string.Empty) is not { } source)
        {
            return false;
        }

        // "TSource" of "Func<TSource, TResult>", but "TKey" of "Func<TKey, IEnumerable<TSource>, TResult>".
        var firstLambdaParameter = GetFirstFunctionArgument(definition.Parameters[lambdaParameter.Ordinal].Type);

        return firstLambdaParameter is not null &&
               GetTypesInside(source, withInterfaces: true).Contains(firstLambdaParameter);
    }

    /// <summary>
    /// The declared type the entities come from: the named parameter, or when none is named, the first parameter
    /// of an extension method (<c>query.Where(...)</c> is <c>Queryable.Where(query, ...)</c>) and the containing
    /// type of anything else.
    /// </summary>
    private static ITypeSymbol? GetDeclaredSource(ISymbol definition, string parameterName)
        => definition switch
        {
            IMethodSymbol method when parameterName.Length > 0
                => method.Parameters.FirstOrDefault(parameter => parameter.Name == parameterName)?.Type,
            IMethodSymbol { IsExtensionMethod: true } method when method.Parameters.Length > 0
                => method.Parameters[0].Type,
            _ => definition.ContainingType,
        };

    /// <summary>
    /// Returns the type and every type it is built from: its type arguments, its array element, the type
    /// arguments of the type it is nested in and, when asked, of the interfaces it implements.
    /// <c>Dictionary&lt;TKey, TValue&gt;.Enumerator</c> is built from TKey and TValue, and a dictionary is also
    /// a sequence of <c>KeyValuePair&lt;TKey, TValue&gt;</c>.
    /// </summary>
    private static HashSet<ITypeSymbol> GetTypesInside(ITypeSymbol type, bool withInterfaces)
    {
        var found = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        Collect(type);
        return found;

        void Collect(ITypeSymbol? current)
        {
            if (current is null || !found.Add(current))
            {
                return;
            }

            if (current is IArrayTypeSymbol array)
            {
                Collect(array.ElementType);
            }

            if (current is not INamedTypeSymbol named)
            {
                return;
            }

            foreach (var argument in named.TypeArguments)
            {
                Collect(argument);
            }

            Collect(named.ContainingType);

            if (withInterfaces)
            {
                foreach (var argument in named.AllInterfaces.SelectMany(implemented => implemented.TypeArguments))
                {
                    Collect(argument);
                }
            }
        }
    }

    /// <summary>
    /// Returns the first type argument of a <c>Func&lt;...&gt;</c>, through the
    /// <c>Expression&lt;Func&lt;...&gt;&gt;</c> that <see cref="IQueryable{T}"/> methods declare.
    /// </summary>
    private static ITypeSymbol? GetFirstFunctionArgument(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol { IsGenericType: true } generic)
        {
            return null;
        }

        if (generic.ConstructedFrom.MetadataName == "Expression`1" &&
            generic.ContainingNamespace?.ToDisplayString() == "System.Linq.Expressions")
        {
            return GetFirstFunctionArgument(generic.TypeArguments[0]);
        }

        return generic.TypeArguments.Length > 0 ? generic.TypeArguments[0] : null;
    }
}
