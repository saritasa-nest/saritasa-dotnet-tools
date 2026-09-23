using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// What a bridge somebody writes is allowed to say.
/// </summary>
/// <remarks>
/// A bridge is a promise that the same entity objects come out of a method that went into it. Nothing the
/// analyzer reads can check that, so the two shapes where it can quietly be false are not allowed at all.
/// <b>Static.</b> An instance method can change what it was called on, or hand back something built from a
/// field, and the annotation would still look right. An extension method counts as static, and the value in
/// front of the dot is its first parameter.
/// <b>Same entity type.</b> A bridge may carry entities into another container but not turn them into
/// something else; otherwise nobody reading the code can say which query a value came from.
/// What breaks a rule is ignored rather than trusted, and reported as INCL006.
/// None of this applies to <see cref="BuiltInBridge"/>, which is ours and is checked by being read. That is
/// why <see cref="EntityType"/> lives in here rather than beside the search.
/// </remarks>
internal static class CustomBridgeRules
{
    /// <summary>
    /// True when the bridge written for this method is allowed.
    /// </summary>
    /// <param name="method">Method the bridge is written for.</param>
    /// <param name="bridge">What the annotation says.</param>
    /// <param name="reason">Why it is not allowed, in a form that finishes "is ignored: ...".</param>
    /// <returns>True when the bridge may be used.</returns>
    public static bool IsAllowed(IMethodSymbol method, Bridge bridge, out string reason)
    {
        var annotation = GetAsWritten(method);

        if (!annotation.IsStatic)
        {
            reason = "only a static method can be annotated to hand back the entities it was given";

            return false;
        }

        var from = GetEndType(annotation, bridge.From);
        var to = GetEndType(annotation, bridge.To);

        if (!EntityType.CarriesSameEntities(from, to))
        {
            reason = "it would carry entities from '" + Describe(to) + "' to '" + Describe(from) +
                     "', and a bridge may change the container but not the entity. If the first of those is " +
                     "a collection of its own, have it implement IEnumerable<T>";

            return false;
        }

        reason = string.Empty;

        return true;
    }

    /// <summary>
    /// The type of the value at one end of the bridge, as the method annotates it, or null where there is
    /// nothing to read.
    /// </summary>
    /// <remarks>
    /// Read from the annotation rather than from a call, so a method is judged on what it promises in
    /// general and not on what one caller happened to put in it.
    /// </remarks>
    private static ITypeSymbol? GetEndType(IMethodSymbol annotation, BridgeEnd end)
        => end switch
        {
            BridgeEnd.Result => annotation.ReturnType,

            // For a static extension method the value in front of the dot is the first parameter. For any
            // other static method there is nothing in front of the dot at all.
            BridgeEnd.Instance => annotation.IsExtensionMethod && annotation.Parameters.Length > 0
                ? annotation.Parameters[0].Type
                : null,

            BridgeEnd.Parameter parameter => GetParameterType(annotation, parameter),

            _ => null,
        };

    /// <summary>
    /// The type of a named parameter, or of one parameter of the callback it takes.
    /// </summary>
    private static ITypeSymbol? GetParameterType(IMethodSymbol annotation, BridgeEnd.Parameter end)
    {
        var parameter = annotation.Parameters
            .FirstOrDefault(candidate => string.Equals(candidate.Name, end.Name, StringComparison.Ordinal));

        if (parameter is null)
        {
            return null;
        }

        if (parameter.Type is not INamedTypeSymbol { DelegateInvokeMethod: { } invoke })
        {
            return parameter.Type;
        }

        return end.LambdaPosition >= 0 && end.LambdaPosition < invoke.Parameters.Length
            ? invoke.Parameters[end.LambdaPosition].Type
            : null;
    }

    /// <summary>
    /// The method as it was written. An extension method called with a dot arrives with its first parameter
    /// already taken off.
    /// </summary>
    private static IMethodSymbol GetAsWritten(IMethodSymbol method)
        => method.ReducedFrom ?? method;

    private static string Describe(ITypeSymbol? type)
        => EntityType.GetEntityType(type)?.ToDisplayString() ?? "?";

    /// <summary>
    /// Reads the entity type out of whatever is holding it, so that the two ends of a bridge somebody wrote can
    /// be compared.
    /// </summary>
    /// <remarks>
    /// Containers are stripped from both sides until neither has one left, and what is left has to be the
    /// same type. Stripping nothing is a normal answer, since a method may hand back one of what it was
    /// given. Which containers exist is the fixed list in <see cref="GetContentByName"/>.
    /// A container of somebody's own is not on the list, so a bridge into one is rejected: it looks the same
    /// as a bridge to an unrelated entity. Making it enumerable is what tells the two apart.
    /// A type that says nothing at all is let through, because there the answer would be noise.
    /// This cannot tell overloads apart, see <see cref="BuiltInBridge.ExcludeOverloadsWithParameters"/>.
    /// </remarks>
    private static class EntityType
    {
        private const string Nullable = "System.Nullable`1";
        private const string Task = "System.Threading.Tasks.Task`1";
        private const string ValueTask = "System.Threading.Tasks.ValueTask`1";
        private const string KeyValuePair = "System.Collections.Generic.KeyValuePair`2";
        private const string Grouping = "System.Linq.IGrouping`2";
        private const string Dictionary = "System.Collections.Generic.IDictionary`2";
        private const string ReadOnlyDictionary = "System.Collections.Generic.IReadOnlyDictionary`2";
        private const string Enumerator = "System.Collections.Generic.IEnumerator`1";
        private const string AsyncEnumerator = "System.Collections.Generic.IAsyncEnumerator`1";
        private const string Enumerable = "System.Collections.Generic.IEnumerable`1";
        private const string AsyncEnumerable = "System.Collections.Generic.IAsyncEnumerable`1";
        private const string OldEnumerable = "System.Collections.IEnumerable";
        private const string OldEnumerator = "System.Collections.IEnumerator";

        /// <summary>
        /// Only here so that a type that somehow contains itself cannot spin.
        /// </summary>
        private const int MaxDepth = 16;

        /// <summary>
        /// True unless the two sides are known to hold different things.
        /// </summary>
        /// <param name="from">Type of the value the search would be standing on.</param>
        /// <param name="to">Type of the value the bridge lands on.</param>
        /// <returns>True when both sides hold the same entity.</returns>
        public static bool CarriesSameEntities(ITypeSymbol? from, ITypeSymbol? to)
        {
            var left = GetEntityType(from);
            var right = GetEntityType(to);

            if (left is null || right is null || SaysNothing(left) || SaysNothing(right))
            {
                return true;
            }

            return SymbolEqualityComparer.Default.Equals(left, right);
        }

        /// <summary>
        /// What a value of this type holds, once every container is stripped off it: for
        /// <c>Task&lt;List&lt;User&gt;&gt;</c>, <c>User</c>.
        /// </summary>
        /// <param name="type">Type to read, which may be null where Roslyn has none.</param>
        /// <returns>Entity type, or null when there was no type to read.</returns>
        public static ITypeSymbol? GetEntityType(ITypeSymbol? type)
        {
            if (type is null)
            {
                return null;
            }

            for (var depth = 0; depth < MaxDepth; depth++)
            {
                if (GetContent(type) is not { } inner)
                {
                    return type;
                }

                type = inner;
            }

            return type;
        }

        /// <summary>
        /// What one container holds, or null when the type is not one.
        /// </summary>
        /// <remarks>
        /// The name is asked before the interfaces: a dictionary is also enumerable, of its pairs rather
        /// than its values, so the other order would take it for the wrong thing.
        /// </remarks>
        private static ITypeSymbol? GetContent(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol array)
            {
                return array.ElementType;
            }

            if (type is not INamedTypeSymbol named || named.IsUnboundGenericType)
            {
                return null;
            }

            if (GetContentByName(named) is { } content)
            {
                return content;
            }

            foreach (var implemented in named.AllInterfaces)
            {
                if (GetContentByName(implemented) is { } fromInterface)
                {
                    return fromInterface;
                }
            }

            return null;
        }

        /// <summary>
        /// What the type holds when it is one of the containers on the list, or null when it is not.
        /// </summary>
        private static ITypeSymbol? GetContentByName(INamedTypeSymbol type)
            => GetFullName(type) switch
            {
                Nullable or Task or ValueTask => type.TypeArguments[0],
                Enumerator or AsyncEnumerator => type.TypeArguments[0],
                Enumerable or AsyncEnumerable => type.TypeArguments[0],
                Dictionary or ReadOnlyDictionary => type.TypeArguments[1],
                KeyValuePair or Grouping => type.TypeArguments[1],
                _ => null,
            };

        /// <summary>
        /// True for a type that names no entity, so that nothing can be concluded from it.
        /// </summary>
        private static bool SaysNothing(ITypeSymbol type)
            => type.SpecialType == SpecialType.System_Object ||
               type.TypeKind is TypeKind.Dynamic or TypeKind.Error ||
               GetFullName(type) is OldEnumerable or OldEnumerator;

        /// <summary>
        /// Namespace and metadata name of the type, spelled the way <see cref="BuiltInBridges"/> spells one.
        /// </summary>
        private static string GetFullName(ITypeSymbol type)
            => type.ContainingNamespace is { IsGlobalNamespace: false } containing
                ? containing.ToDisplayString() + "." + type.MetadataName
                : type.MetadataName;
    }
}
