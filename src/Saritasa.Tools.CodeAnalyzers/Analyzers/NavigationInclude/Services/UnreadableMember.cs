using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// The member that stopped the search, carried on an INCL004 diagnostic so that the code fix can offer to
/// declare it and the message can name it.
/// </summary>
internal static class UnreadableMember
{
    /// <summary>
    /// Key of the declaring type on the diagnostic.
    /// </summary>
    public const string TypeKey = "UnreadableType";

    /// <summary>
    /// Key of the member name on the diagnostic.
    /// </summary>
    public const string MemberKey = "UnreadableMember";

    /// <summary>
    /// Key of the parameter the entities would come from, empty for a property or an instance method.
    /// </summary>
    public const string ParameterKey = "UnreadableParameter";

    /// <summary>
    /// Returns how to name the member in a message, e.g. "SomeLib.QueryExtensions.Paginate".
    /// </summary>
    /// <param name="unknownSource">The value the search could not read.</param>
    /// <returns>Name for the message.</returns>
    public static string Describe(IOperation? unknownSource)
    {
        if (FindMember(unknownSource) is not { } member)
        {
            return "the value";
        }

        return member.ContainingType is { } containingType
            ? containingType.ToDisplayString() + "." + member.Name
            : member.Name;
    }

    /// <summary>
    /// Returns what the code fix needs to write a declaration for the member, and an empty set when the search
    /// stopped at something no declaration can describe.
    /// </summary>
    /// <param name="unknownSource">The value the search could not read.</param>
    /// <returns>Properties for the code fix.</returns>
    public static ImmutableDictionary<string, string?> GetProperties(IOperation? unknownSource)
    {
        if (FindMember(unknownSource) is not { ContainingType: { } containingType } member)
        {
            return ImmutableDictionary<string, string?>.Empty;
        }

        return ImmutableDictionary<string, string?>.Empty
            .Add(TypeKey, GetUnboundTypeName(containingType))
            .Add(MemberKey, member.Name)
            .Add(ParameterKey, GetSourceParameterName(member));
    }

    /// <summary>
    /// The name to write inside "typeof(...)". A generic type has to be written unbound there, as
    /// <c>PagedResult&lt;&gt;</c> rather than <c>PagedResult&lt;T&gt;</c>.
    /// </summary>
    private static string GetUnboundTypeName(INamedTypeSymbol type)
    {
        var definition = type.OriginalDefinition;

        var withoutTypeArguments = definition.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
                .WithGenericsOptions(SymbolDisplayGenericsOptions.None));

        return definition.Arity == 0
            ? withoutTypeArguments
            : withoutTypeArguments + "<" + new string(',', definition.Arity - 1) + ">";
    }

    /// <summary>
    /// The member the value came from: the called method, or the property that was read.
    /// </summary>
    private static ISymbol? FindMember(IOperation? unknownSource)
        => unknownSource switch
        {
            IInvocationOperation call => call.TargetMethod.OriginalDefinition,
            IPropertyReferenceOperation reference => reference.Property.OriginalDefinition,
            _ => null,
        };

    /// <summary>
    /// The parameter a declaration would name. An extension method takes its source as the first parameter;
    /// anything else takes it from the object it is called on, which a declaration writes as no parameter.
    /// </summary>
    private static string GetSourceParameterName(ISymbol member)
        => member is IMethodSymbol { IsExtensionMethod: true } method && method.Parameters.Length > 0
            ? method.Parameters[0].Name
            : string.Empty;
}
