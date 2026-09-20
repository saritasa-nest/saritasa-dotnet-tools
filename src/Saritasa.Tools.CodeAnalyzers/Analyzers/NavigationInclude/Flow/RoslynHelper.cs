using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Batch of functions which hides roslyn details.
/// </summary>
internal static class RoslynHelper
{
    /// <summary>
    /// Removes conversion and delegate creation wrappers around an operation.
    /// </summary>
    /// <param name="operation">Operation.</param>
    /// <returns>Operation without the wrappers.</returns>
    public static IOperation SkipWrappers(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => SkipWrappers(conversion.Operand),
            IDelegateCreationOperation delegateCreation => SkipWrappers(delegateCreation.Target),
            _ => operation,
        };

    /// <summary>
    /// The value the method is called on: <c>Instance</c> for instance methods, the first argument for
    /// extension methods (<c>query.Where(...)</c> is <c>Queryable.Where(query, ...)</c>).
    /// </summary>
    /// <param name="call">Method call.</param>
    /// <returns>Receiver or null.</returns>
    public static IOperation? GetReceiver(IInvocationOperation call)
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
    /// The declared type a member is used on: the first parameter of an extension method, the containing type
    /// of anything else.
    /// </summary>
    /// <param name="member">Method or property, as it is declared.</param>
    /// <returns>Type.</returns>
    public static ITypeSymbol? GetReceiverType(ISymbol member)
        => member is IMethodSymbol { IsExtensionMethod: true, Parameters.Length: > 0 } extension
            ? extension.Parameters[0].Type
            : member.ContainingType;

    /// <summary>
    /// The type and every type it is built from: its type arguments, its array element, the type arguments of
    /// the type it is nested in and, when asked, of the interfaces it implements.
    /// <c>Dictionary&lt;TKey, TValue&gt;.Enumerator</c> is built from TKey and TValue, and a dictionary is also
    /// a sequence of <c>KeyValuePair&lt;TKey, TValue&gt;</c>.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="withInterfaces">True to read the type arguments of the implemented interfaces too.</param>
    /// <returns>Types.</returns>
    public static HashSet<ITypeSymbol> GetTypesInside(ITypeSymbol type, bool withInterfaces)
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
    /// The declared type of one parameter of a delegate, read from the delegate itself so that
    /// <c>Func&lt;...&gt;</c> and <c>Action&lt;...&gt;</c> need no rule of their own. Reads through the
    /// <c>Expression&lt;Func&lt;...&gt;&gt;</c> that <see cref="IQueryable{T}"/> methods declare.
    /// </summary>
    /// <param name="type">Declared type of the parameter the lambda is passed to.</param>
    /// <param name="ordinal">Position of the lambda parameter in question.</param>
    /// <returns>Type or null.</returns>
    public static ITypeSymbol? GetLambdaParameterType(ITypeSymbol type, int ordinal)
    {
        if (type is not INamedTypeSymbol { IsGenericType: true } generic)
        {
            return null;
        }

        if (generic.ConstructedFrom.MetadataName == "Expression`1" &&
            generic.ContainingNamespace?.ToDisplayString() == "System.Linq.Expressions")
        {
            return GetLambdaParameterType(generic.TypeArguments[0], ordinal);
        }

        var parameters = generic.DelegateInvokeMethod?.Parameters ?? default;

        return ordinal < parameters.Length ? parameters[ordinal].Type : null;
    }
}
