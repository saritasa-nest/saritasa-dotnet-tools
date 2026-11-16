using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;

/// <summary>
/// The member that stopped the search, carried on an INCL004 diagnostic so that the code fix can offer to
/// annotate it and the message can name it.
/// </summary>
internal static class UnreadableMember
{
    /// <summary>
    /// Key of the declaring type on the diagnostic, as written inside "typeof(...)".
    /// </summary>
    public const string TypeKey = "UnreadableType";

    /// <summary>
    /// Key of the method name on the diagnostic.
    /// </summary>
    public const string MethodKey = "UnreadableMethod";

    /// <summary>
    /// Key of the method's documentation id, which finds the method again in the code fix, so that an
    /// annotation can go on the method itself when its source is in the solution.
    /// </summary>
    public const string MethodIdKey = "UnreadableMethodId";

    /// <summary>
    /// Key of the parameters the entities could come from, separated by <see cref="ParameterSeparator"/>.
    /// </summary>
    public const string ParametersKey = "UnreadableParameters";

    /// <summary>
    /// Key of the callback parameter the entities arrived through. Absent when they arrived at the result.
    /// </summary>
    public const string CallbackKey = "UnreadableCallback";

    /// <summary>
    /// Key of the position inside the callback the entities arrived at. Absent when they arrived at the result.
    /// </summary>
    public const string CallbackPositionKey = "UnreadableCallbackPosition";

    /// <summary>
    /// Separates the names under <see cref="ParametersKey"/>. A parameter name cannot contain it.
    /// </summary>
    public const char ParameterSeparator = ',';

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
    /// A fix is offered only when the annotation would be allowed and would let the search go on. So nothing
    /// for a property, which no annotation can describe; nothing for a method that is already annotated, since
    /// a second line would not help either; and nothing when no parameter holds the same entity, which covers
    /// an instance method and a change of entity type alike, both of which the rules refuse.
    /// </remarks>
    /// <param name="unknown">Where the search stopped.</param>
    /// <param name="bridges">Every bridge the compilation can see.</param>
    /// <returns>Properties for the code fix.</returns>
    public static ImmutableDictionary<string, string?> GetProperties(Answer.UnknownAnswer unknown, Bridges bridges)
    {
        if (FindMember(unknown.UnreadableValue) is not IMethodSymbol { ContainingType: { } containingType } method ||
            IsAnnotated(method, unknown.From, bridges))
        {
            return ImmutableDictionary<string, string?>.Empty;
        }

        var parameters = GetSourceParameters(method, unknown.From);
        if (parameters.Count == 0)
        {
            return ImmutableDictionary<string, string?>.Empty;
        }

        var properties = ImmutableDictionary<string, string?>.Empty
            .Add(TypeKey, GetUnboundTypeName(containingType))
            .Add(MethodKey, method.Name)
            .Add(MethodIdKey, DocumentationCommentId.CreateDeclarationId(method))
            .Add(ParametersKey, string.Join(ParameterSeparator.ToString(), parameters));

        return unknown.From is BridgeEnd.Parameter callback
            ? properties
                .Add(CallbackKey, callback.Name)
                .Add(CallbackPositionKey, callback.LambdaPosition.ToString(CultureInfo.InvariantCulture))
            : properties;
    }

    /// <summary>
    /// True when a bridge already runs from that end of the method, and only could not be crossed at this call.
    /// </summary>
    private static bool IsAnnotated(IMethodSymbol method, BridgeEnd from, Bridges bridges)
        => from is BridgeEnd.Parameter callback
            ? bridges.FindFromParameter(method, callback.Name, callback.LambdaPosition).Count > 0
            : bridges.FindFromResult(method).Count > 0;

    /// <summary>
    /// The parameters an annotation could name: every one that holds the same entity as the end the search
    /// stands on. A callback and an out parameter are where entities arrive, never where they come from.
    /// </summary>
    private static IReadOnlyList<string> GetSourceParameters(IMethodSymbol method, BridgeEnd from)
        => method.Parameters
            .Where(parameter =>
                parameter.RefKind != RefKind.Out &&
                parameter.Type is not INamedTypeSymbol { DelegateInvokeMethod: not null } &&
                CustomBridgeRules.IsSuggestable(method, new Bridge(from, new BridgeEnd.Parameter(parameter.Name))))
            .Select(parameter => parameter.Name)
            .ToList();

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
}
