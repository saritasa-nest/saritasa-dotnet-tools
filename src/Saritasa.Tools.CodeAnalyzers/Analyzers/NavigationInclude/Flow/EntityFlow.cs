using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Reads from a member's declaration how entities move through it.
/// </summary>
/// <remarks>
/// Which members pass entities on is decided by [PreservesIncludes] declarations, the built-in ones and the
/// project's own. This class answers the two questions a declaration cannot: whether one particular overload of
/// a declared member really hands the entities back, and which lambda parameters receive elements of a
/// collection. Both are read from how the member is declared, so they work the same for System.Linq, EF Core
/// and a project's own methods.
/// </remarks>
internal static class EntityFlow
{
    /// <summary>
    /// Returns the collection or query the method is called on: <c>Instance</c> for instance methods,
    /// the first argument for extension methods (<c>query.Where(...)</c> is <c>Queryable.Where(query, ...)</c>).
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? GetSource(IInvocationOperation call)
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
    /// <param name="member">Declared method or property.</param>
    /// <param name="parameterName">Declared parameter; empty for the value the member is used on.</param>
    /// <returns>True if the entities of the source can come out of the member.</returns>
    public static bool HandsBackSource(ISymbol member, string parameterName)
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
    /// For <c>users.Select(u =&gt; ...)</c> returns <c>users</c>: the collection whose elements the lambda
    /// receives. Null if the lambda receives something else.
    /// </summary>
    /// <param name="lambda">Lambda.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? GetContainer(IFlowAnonymousFunctionOperation lambda)
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
