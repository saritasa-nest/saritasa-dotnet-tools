using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// Every <see cref="Bridge"/> the compilation can see, ready to look up by member.
/// </summary>
/// <remarks>
/// A bridge comes from one of three places: <see cref="BuiltInBridges"/>, an
/// <c>[assembly: PassesIncludes]</c>, and the member itself. Reading the references is the expensive part,
/// so the first two are read once per compilation and the third per lookup.
/// They are kept apart because they may not say the same things: only a built-in line names overloads or
/// speaks for a property, and only a built-in line describes a method that is not static, see
/// <see cref="CustomBridgeRules"/>. Nothing here is inferred; a member nobody annotated is not followed.
/// A lookup asks with the end the search stands on and gets back the ends to move to. Two of them mean the
/// entities could have come from either place, as for <c>first.Concat(second)</c>.
/// </remarks>
internal sealed class Bridges
{
    /// <summary>
    /// The built-in table, with every declaring type looked up in the compilation, grouped by member name so
    /// that a lookup only compares types for members with the right name.
    /// </summary>
    private readonly ILookup<string, ResolvedBuiltIn> builtIn;

    /// <summary>
    /// What assembly attributes annotated, grouped the same way.
    /// </summary>
    private readonly ILookup<string, CustomBridge> custom;

    private Bridges(IEnumerable<ResolvedBuiltIn> builtIn, IEnumerable<CustomBridge> custom)
    {
        this.builtIn = builtIn.ToLookup(resolved => resolved.Bridge.MemberName, StringComparer.Ordinal);
        this.custom = custom.ToLookup(bridge => bridge.MemberName, StringComparer.Ordinal);
    }

    /// <summary>
    /// Reads every bridge the compilation can see.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>Bridges.</returns>
    public static Bridges Read(Compilation compilation)
    {
        var builtIn = BuiltInBridges.All
            .SelectMany(bridge => compilation
                .GetTypesByMetadataName(bridge.DeclaringType)
                .Select(type => new ResolvedBuiltIn(type, bridge)));

        var custom = new[] { compilation.Assembly }
            .Concat(compilation.SourceModule.ReferencedAssemblySymbols)
            .SelectMany(assembly => assembly.GetAttributes())
            .Select(PassesIncludesReader.ReadAssemblyAttribute)
            .OfType<CustomBridge>();

        return new Bridges(builtIn, custom);
    }

    /// <summary>
    /// Where a bridge out of this member's result lands.
    /// </summary>
    /// <param name="member">Method or property the search is standing on.</param>
    /// <returns>Ends to move to. Empty when no bridge runs from there.</returns>
    public IReadOnlyList<BridgeEnd> FindFromResult(ISymbol member)
        => Find(member, from: new BridgeEnd.Result());

    /// <summary>
    /// Where a bridge out of something passed to this method lands. One question covers an out argument and
    /// a callback parameter alike, since a parameter is never both.
    /// </summary>
    /// <param name="method">Method the entities were passed to.</param>
    /// <param name="parameterName">Name of the parameter they arrived through.</param>
    /// <param name="lambdaPosition">Position inside the callback, when the parameter takes one.</param>
    /// <returns>Ends to move to. Empty when no bridge runs from there.</returns>
    public IReadOnlyList<BridgeEnd> FindFromParameter(
        IMethodSymbol method,
        string parameterName,
        int lambdaPosition = 0)
        => Find(method, new BridgeEnd.Parameter(parameterName, lambdaPosition));

    /// <summary>
    /// True when both ends mean the same place at a call.
    /// </summary>
    private static bool SameEnd(BridgeEnd left, BridgeEnd right)
        => (left, right) switch
        {
            (BridgeEnd.Result, BridgeEnd.Result) => true,
            (BridgeEnd.Instance, BridgeEnd.Instance) => true,
            (BridgeEnd.Parameter first, BridgeEnd.Parameter second)
                => string.Equals(first.Name, second.Name, StringComparison.Ordinal) &&
                   first.LambdaPosition == second.LambdaPosition,
            _ => false,
        };

    /// <summary>
    /// True if the member is one of the overloads the line does not describe.
    /// </summary>
    private static bool ExcludesOverloadOf(BuiltInBridge bridge, ISymbol member)
        => member is IMethodSymbol method &&
           bridge.ExcludeOverloadsWithParameters.Any(excluded =>
               method.Parameters.Any(parameter =>
                   string.Equals(parameter.Name, excluded, StringComparison.Ordinal)));

    /// <summary>
    /// True if the symbol is declared by the type, or by one deriving from it or implementing it: "foreach"
    /// over a List calls the List method, and it still has to count as the IEnumerable one.
    /// </summary>
    private static bool IsDeclaredBy(ISymbol symbol, INamedTypeSymbol declaringType)
    {
        if (symbol.ContainingType is not { } containingType)
        {
            return false;
        }

        return GetBaseTypes(containingType)
            .Concat(containingType.AllInterfaces)
            .Any(type => SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, declaringType));
    }

    private static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            yield return current;
        }
    }

    /// <summary>
    /// Everything annotated under the member's name.
    /// </summary>
    /// <remarks>
    /// An indexer is named "this[]" in C# and "Item" in metadata, so both spellings are asked for.
    /// </remarks>
    private static IEnumerable<T> GetCandidates<T>(ILookup<string, T> annotated, ISymbol definition)
        => annotated[definition.Name].Concat(
            definition.MetadataName == definition.Name
                ? []
                : annotated[definition.MetadataName]);

    /// <summary>
    /// Every bridge of this member running from the given end, from the first place that annotates any. The
    /// member itself has the last word, then the built-in table, then an assembly attribute.
    /// </summary>
    private IReadOnlyList<BridgeEnd> Find(ISymbol member, BridgeEnd from)
    {
        var definition = member.OriginalDefinition;

        var custom = Collect(
            definition.GetAttributes()
                .Select(PassesIncludesReader.ReadMemberAttribute)
                .OfType<Bridge>()
                .Where(bridge => IsAllowedAnnotation(definition, bridge)),
            from);

        if (custom.Count > 0)
        {
            return custom;
        }

        var annotated = FindBuiltIn(definition, from);

        return annotated.Count > 0 ? annotated : FindAssemblyCustom(definition, from);
    }

    /// <summary>
    /// The same, among the bridges that ship with the analyzer.
    /// </summary>
    private IReadOnlyList<BridgeEnd> FindBuiltIn(ISymbol definition, BridgeEnd from)
        => Collect(
            GetCandidates(builtIn, definition)
                .Where(resolved =>
                    !ExcludesOverloadOf(resolved.Bridge, definition) &&
                    IsDeclaredBy(definition, resolved.Type))
                .Select(resolved => resolved.Bridge.Bridge),
            from);

    /// <summary>
    /// The same, among the bridges annotated by an assembly attribute.
    /// </summary>
    private IReadOnlyList<BridgeEnd> FindAssemblyCustom(ISymbol definition, BridgeEnd from)
        => Collect(
            GetCandidates(custom, definition)
                .Where(annotated => IsDeclaredBy(definition, annotated.DeclaringType))
                .Select(annotated => annotated.Bridge)
                .Where(bridge => IsAllowedAnnotation(definition, bridge)),
            from);

    /// <summary>
    /// True when a bridge somebody wrote may be used for this member. Only a method: a property named by
    /// string is ignored, so that every property bridge is one we can verify by reading it. What the rules
    /// refuse is ignored here and reported as INCL006 where it was written.
    /// </summary>
    private static bool IsAllowedAnnotation(ISymbol definition, Bridge bridge)
        => definition is IMethodSymbol method && CustomBridgeRules.IsAllowed(method, bridge, out _);

    /// <summary>
    /// The ends the given bridges land on, counting each place once. A Dictionary is both an IDictionary and
    /// an IReadOnlyDictionary, so its TryGetValue matches two lines that mean the same one place.
    /// </summary>
    private static IReadOnlyList<BridgeEnd> Collect(IEnumerable<Bridge> bridges, BridgeEnd from)
    {
        var ends = new List<BridgeEnd>();

        foreach (var bridge in bridges)
        {
            if (SameEnd(bridge.From, from) && !ends.Any(end => SameEnd(end, bridge.To)))
            {
                ends.Add(bridge.To);
            }
        }

        return ends;
    }

    /// <summary>
    /// One <see cref="BuiltInBridge"/> whose declaring type has been looked up in the compilation.
    /// </summary>
    private sealed class ResolvedBuiltIn
    {
        public ResolvedBuiltIn(INamedTypeSymbol type, BuiltInBridge bridge)
        {
            Type = type;
            Bridge = bridge;
        }

        /// <summary>
        /// Type the bridge was annotated on.
        /// </summary>
        public INamedTypeSymbol Type { get; }

        /// <summary>
        /// The line itself: the member, the move, and which overloads it describes.
        /// </summary>
        public BuiltInBridge Bridge { get; }
    }
}
