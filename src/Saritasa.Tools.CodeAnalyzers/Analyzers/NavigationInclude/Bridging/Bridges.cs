using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// Every <see cref="Bridge"/> the compilation can see, ready to look up by member.
/// </summary>
/// <remarks>
/// A bridge comes from one of three places, treated the same: the member itself, an
/// <c>[assembly: PassesIncludes]</c> the compilation can see, and <see cref="BuiltInBridges"/>. Reading the
/// references is the expensive part, so this is built once per compilation; attributes on the member itself
/// are read per lookup, since pre-reading them would mean walking every referenced assembly.
/// Nothing here is inferred. A member nobody declared is not followed.
/// </remarks>
internal sealed class Bridges
{
    /// <summary>
    /// Nothing is declared anywhere. Used where a compilation is not available.
    /// </summary>
    public static readonly Bridges None = new([]);

    /// <summary>
    /// Grouped by member name, so a lookup only compares types for members with the right name.
    /// </summary>
    private readonly ILookup<string, DeclaredBridge> bridges;

    private Bridges(IEnumerable<DeclaredBridge> bridges)
    {
        this.bridges = bridges.ToLookup(declared => declared.Bridge.MemberName, StringComparer.Ordinal);
    }

    /// <summary>
    /// Reads every bridge the compilation can see.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>Bridges.</returns>
    public static Bridges Read(Compilation compilation)
    {
        var builtIn = BuiltInBridges.All
            .SelectMany(line => compilation
                .GetTypesByMetadataName(line.DeclaringType)
                .Select(type => new DeclaredBridge(type, line.Bridge, isBuiltIn: true)));

        var written = new[] { compilation.Assembly }
            .Concat(compilation.SourceModule.ReferencedAssemblySymbols)
            .SelectMany(assembly => assembly.GetAttributes())
            .Select(ReadAssemblyAttribute)
            .OfType<DeclaredBridge>();

        return new Bridges(builtIn.Concat(written));
    }

    /// <summary>
    /// Where a bridge out of this member's result lands, or null when no bridge runs from there.
    /// </summary>
    /// <param name="member">Method or property the search is standing on.</param>
    /// <returns>Parameter name, an empty string, or null.</returns>
    public string? FindFromResult(ISymbol member)
        => Find(member, bridge => bridge is Bridge.FromResult);

    /// <summary>
    /// Where a bridge out of a callback's parameter lands, or null when no bridge runs from there.
    /// </summary>
    /// <param name="method">Method the callback is passed to.</param>
    /// <param name="callback">Name of the method parameter that takes the callback.</param>
    /// <param name="parameter">Position of the callback's own parameter.</param>
    /// <returns>Parameter name, an empty string, or null.</returns>
    public string? FindFromLambdaParameter(IMethodSymbol method, string callback, int parameter)
        => Find(
            method,
            bridge => bridge is Bridge.FromLambdaParameter lambda &&
                      string.Equals(lambda.Callback, callback, StringComparison.Ordinal) &&
                      lambda.Parameter == parameter);

    /// <summary>
    /// Where a bridge out of an out argument lands, or null when no bridge runs from there.
    /// </summary>
    /// <param name="method">Method that wrote the out argument.</param>
    /// <param name="parameter">Name of the out parameter.</param>
    /// <returns>Parameter name, an empty string, or null.</returns>
    public string? FindFromOutArgument(IMethodSymbol method, string parameter)
        => Find(
            method,
            bridge => bridge is Bridge.FromOutArgument outArgument &&
                      string.Equals(outArgument.Parameter, parameter, StringComparison.Ordinal));

    /// <summary>
    /// The first bridge of this member the search is asking for, written on the member or in the table.
    /// </summary>
    private string? Find(ISymbol member, Func<Bridge, bool> isTheOneWanted)
    {
        var definition = member.OriginalDefinition;

        foreach (var attribute in definition.GetAttributes())
        {
            if (ReadMemberAttribute(attribute, definition.Name) is { } written && isTheOneWanted(written))
            {
                return written.Source;
            }
        }

        // An indexer is named "this[]" in C# and "Item" in metadata.
        var candidates = bridges[definition.Name].Concat(
            definition.MetadataName == definition.Name ? [] : bridges[definition.MetadataName]);

        foreach (var declared in candidates)
        {
            var bridge = declared.Bridge;

            // An assembly attribute naming a property is ignored, so every property bridge is verifiable.
            if (!isTheOneWanted(bridge) ||
                (!declared.IsBuiltIn && definition is IPropertySymbol) ||
                bridge.ExcludesOverloadOf(definition) ||
                !IsDeclaredBy(definition, declared.Type))
            {
                continue;
            }

            return bridge.Source;
        }

        return null;
    }

    /// <summary>
    /// True if the symbol is declared by the type, or by one deriving from it or implementing it. A name on
    /// an implementing type is enough: "foreach" over a List calls List's own GetEnumerator, not the
    /// interface method, and it still has to count as IEnumerable's.
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
    /// A [PassesIncludes] written on the member itself, which names its own parameters with nameof.
    /// </summary>
    private static Bridge? ReadMemberAttribute(AttributeData attribute, string memberName)
        => IsPassesIncludes(attribute)
            ? CreateBridge(memberName, GetStringArgument(attribute, 0) ?? string.Empty, attribute)
            : null;

    /// <summary>
    /// An <c>[assembly: PassesIncludes(typeof(SomeType), "Member", "source")]</c>, which names someone else's
    /// member by string.
    /// </summary>
    private static DeclaredBridge? ReadAssemblyAttribute(AttributeData attribute)
    {
        if (!IsPassesIncludes(attribute) ||
            attribute.ConstructorArguments.Length < 2 ||
            attribute.ConstructorArguments[0].Value is not INamedTypeSymbol declaringType ||
            GetStringArgument(attribute, 1) is not { } memberName)
        {
            return null;
        }

        var bridge = CreateBridge(memberName, GetStringArgument(attribute, 2) ?? string.Empty, attribute);

        return new DeclaredBridge(declaringType.OriginalDefinition, bridge, isBuiltIn: false);
    }

    /// <summary>
    /// The attribute can describe a result or a callback; it has no way to name an out parameter.
    /// </summary>
    private static Bridge CreateBridge(string memberName, string source, AttributeData attribute)
        => GetNamedArgument(attribute, nameof(PassesIncludesAttribute.ToCallback)) is string callback
            ? new Bridge.FromLambdaParameter(
                memberName,
                source,
                callback,
                GetNamedArgument(attribute, nameof(PassesIncludesAttribute.ToCallbackParameter)) as int? ?? 0)
            : new Bridge.FromResult(memberName, source);

    private static bool IsPassesIncludes(AttributeData attribute)
        => string.Equals(
            attribute.AttributeClass?.Name,
            nameof(PassesIncludesAttribute),
            StringComparison.Ordinal);

    private static object? GetNamedArgument(AttributeData attribute, string name)
        => attribute.NamedArguments
            .FirstOrDefault(argument => string.Equals(argument.Key, name, StringComparison.Ordinal))
            .Value.Value;

    private static string? GetStringArgument(AttributeData attribute, int index)
        => attribute.ConstructorArguments.Length > index
            ? attribute.ConstructorArguments[index].Value as string
            : null;

    /// <summary>
    /// A bridge together with the type it was declared on, once that type is resolved against the compilation.
    /// The declaring type is the only thing a built-in line and an assembly attribute say differently, so it
    /// is the only thing kept beside the bridge itself.
    /// </summary>
    private sealed class DeclaredBridge
    {
        public DeclaredBridge(INamedTypeSymbol type, Bridge bridge, bool isBuiltIn)
        {
            Type = type;
            Bridge = bridge;
            IsBuiltIn = isBuiltIn;
        }

        /// <summary>
        /// Type the bridge was declared on.
        /// </summary>
        public INamedTypeSymbol Type { get; }

        /// <summary>
        /// The bridge itself.
        /// </summary>
        public Bridge Bridge { get; }

        /// <summary>
        /// True for a line that ships with the analyzer rather than one somebody wrote.
        /// </summary>
        public bool IsBuiltIn { get; }
    }
}
