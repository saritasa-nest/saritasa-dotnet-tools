using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;

/// <summary>
/// The member that stopped the search, carried on an INCL004 diagnostic so that the code fix can offer to
/// annotate it and the message can name it.
/// </summary>
internal static class UnreadableMember
{
    /// <summary>
    /// Key of the declaring type on the diagnostic.
    /// </summary>
    public const string TypeKey = "UnreadableType";

    /// <summary>
    /// Key of the method name on the diagnostic.
    /// </summary>
    public const string MethodKey = "UnreadableMethod";

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
    /// Returns what the code fix needs to write an annotation for the method, and an empty set when there is
    /// no annotation worth offering.
    /// </summary>
    /// <remarks>
    /// Two cases where a fix would not help: a property, which no annotation can describe; and a method the
    /// rules would refuse, which swaps this warning for an INCL006.
    /// </remarks>
    /// <param name="unknownSource">The value the search could not read.</param>
    /// <returns>Properties for the code fix.</returns>
    public static ImmutableDictionary<string, string?> GetProperties(IOperation? unknownSource)
    {
        if (FindMember(unknownSource) is not IMethodSymbol { ContainingType: { } containingType } method)
        {
            return ImmutableDictionary<string, string?>.Empty;
        }

        var parameterName = GetSourceParameterName(method);

        if (!CustomBridgeRules.IsAllowed(method, BuildBridge(parameterName), out _))
        {
            return ImmutableDictionary<string, string?>.Empty;
        }

        return ImmutableDictionary<string, string?>.Empty
            .Add(TypeKey, GetUnboundTypeName(containingType))
            .Add(MethodKey, method.Name)
            .Add(ParameterKey, parameterName);
    }

    /// <summary>
    /// The bridge the code fix would write: out of the result, into the named parameter or the instance.
    /// </summary>
    private static Bridge BuildBridge(string parameterName)
        => new(
            new BridgeEnd.Result(),
            parameterName.Length == 0
                ? new BridgeEnd.Instance()
                : new BridgeEnd.Parameter(parameterName));

    /// <summary>
    /// The name to write inside "typeof(...)", unbound: <c>PagedResult&lt;&gt;</c>, not
    /// <c>PagedResult&lt;T&gt;</c>.
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
    /// The parameter an annotation would name: the first one for an extension method, none otherwise.
    /// </summary>
    private static string GetSourceParameterName(IMethodSymbol method)
        => method is { IsExtensionMethod: true, Parameters.Length: > 0 }
            ? method.Parameters[0].Name
            : string.Empty;
}
