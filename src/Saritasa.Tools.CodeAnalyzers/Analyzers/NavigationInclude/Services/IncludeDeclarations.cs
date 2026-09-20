using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// Every [PreservesIncludes] the compilation can see, ready to look up by member.
/// </summary>
/// <remarks>
/// This is the only place that decides which members pass entities on. A declaration comes from one of three
/// places, and all of them are treated the same:
/// the member itself, an <c>[assembly: PreservesIncludes]</c> in the project or in anything it references, and
/// the built-in list below for System.Linq, EF Core and the collection types.
/// Reading the references is the expensive part, so this is built once per compilation.
/// </remarks>
internal sealed class IncludeDeclarations
{
    /// <summary>
    /// Nothing is declared anywhere. Used where a compilation is not available.
    /// </summary>
    public static readonly IncludeDeclarations None = new([]);

    private static readonly string[] sequenceOperators =
    [
        "Where", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Order", "OrderDescending",
        "Skip", "SkipLast", "SkipWhile", "Take", "TakeLast", "TakeWhile", "Distinct", "DistinctBy", "Reverse",
        "Concat", "Union", "UnionBy", "Intersect", "IntersectBy", "Except", "ExceptBy", "Append", "Prepend",
        "DefaultIfEmpty", "First", "FirstOrDefault", "Single", "SingleOrDefault", "Last", "LastOrDefault",
        "ElementAt", "ElementAtOrDefault", "Min", "Max", "MinBy", "MaxBy",
    ];

    private static readonly string[] entityFrameworkOperators =
    [
        "Include", "ThenInclude", "AsNoTracking", "AsNoTrackingWithIdentityResolution", "AsTracking",
        "AsSplitQuery", "AsSingleQuery", "IgnoreQueryFilters", "IgnoreAutoIncludes", "TagWith", "TagWithCallSite",
        "AsAsyncEnumerable", "FirstAsync", "FirstOrDefaultAsync", "SingleAsync", "SingleOrDefaultAsync",
        "LastAsync", "LastOrDefaultAsync", "ElementAtAsync", "ElementAtOrDefaultAsync", "MinAsync", "MaxAsync",
        "ToListAsync", "ToArrayAsync", "ToHashSetAsync", "ToDictionaryAsync",
    ];

    /// <summary>
    /// Declarations written on assemblies or built in, grouped by member name so that a lookup only compares
    /// types for members with the right name.
    /// </summary>
    private readonly ILookup<string, TypeDeclaration> declarations;

    private IncludeDeclarations(IEnumerable<TypeDeclaration> declarations)
    {
        this.declarations = declarations.ToLookup(declaration => declaration.MemberName, StringComparer.Ordinal);
    }

    /// <summary>
    /// The declarations that ship with the analyzer: the metadata name of the declaring type and the member name.
    /// </summary>
    /// <remarks>
    /// Each line means exactly what <c>[assembly: PreservesIncludes(typeof(Type), "Member")]</c> means: the member
    /// hands back the entities of the value it is used on. They go through the same lookup as a project's own
    /// declarations, so nothing about Microsoft's methods is special in the search.
    /// Types are named by their metadata name rather than with typeof, so the analyzer needs no reference to EF Core.
    /// A line whose type the project does not reference simply matches nothing.
    /// A member declared on an interface also covers the classes implementing it, so <c>IList`1</c> covers the
    /// indexer of <c>List&lt;T&gt;</c> and <c>IEnumerable`1</c> covers the <c>GetEnumerator</c> that
    /// <c>foreach</c> calls on any collection.
    /// Select, SelectMany and anything else that makes new objects are left out on purpose. Overloads that make new
    /// objects are filtered out anyway: see <see cref="Flow.Transformation"/>.
    /// </remarks>
    private static IEnumerable<(string Type, string Member)> BuiltInEntries
        => Declare("System.Linq.Enumerable", [.. sequenceOperators, "AsEnumerable", "ToList", "ToArray", "ToHashSet", "ToDictionary"])
            .Concat(Declare("System.Linq.Queryable", [.. sequenceOperators, "AsQueryable"]))
            .Concat(Declare("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions", entityFrameworkOperators))
            .Concat(Declare("Microsoft.EntityFrameworkCore.DbSet`1", ["AsAsyncEnumerable", "AsQueryable"]))

            // List has its own instance methods, which win over the Enumerable ones of the same name.
            .Concat(Declare("System.Collections.Generic.List`1", ["ToArray", "Find", "FindLast", "FindAll", "GetRange", "AsReadOnly"]))

            // "foreach" is rewritten by the compiler into GetEnumerator and Current.
            .Concat(Declare("System.Collections.Generic.IEnumerable`1", ["GetEnumerator"]))
            .Concat(Declare("System.Collections.Generic.IAsyncEnumerable`1", ["GetAsyncEnumerator"]))
            .Concat(Declare("System.Collections.Generic.IEnumerator`1", ["Current"]))
            .Concat(Declare("System.Collections.Generic.IAsyncEnumerator`1", ["Current"]))

            // "foreach" over an array goes through the old non-generic interfaces.
            .Concat(Declare("System.Collections.IEnumerable", ["GetEnumerator"]))
            .Concat(Declare("System.Collections.IEnumerator", ["Current"]))

            // "users[0]", "users[id]", "users.Values", "pair.Value", "users.GetValueOrDefault(id)",
            // "users.TryGetValue(id, out var user)".
            .Concat(Declare("System.Collections.Generic.IList`1", ["this[]"]))
            .Concat(Declare("System.Collections.Generic.IReadOnlyList`1", ["this[]"]))
            .Concat(Declare("System.Collections.Generic.IDictionary`2", ["this[]", "Values", "TryGetValue"]))
            .Concat(Declare("System.Collections.Generic.IReadOnlyDictionary`2", ["this[]", "Values", "TryGetValue"]))
            .Concat(Declare("System.Collections.Generic.KeyValuePair`2", ["Value"]))
            .Concat(Declare("System.Collections.Generic.CollectionExtensions", ["GetValueOrDefault"]))
            .Concat(Declare("System.Threading.Tasks.Task`1", ["Result"]));

    /// <summary>
    /// Reads every declaration the compilation can see.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>Declarations.</returns>
    public static IncludeDeclarations Read(Compilation compilation)
    {
        var builtIn = BuiltInEntries
            .SelectMany(entry => compilation
                .GetTypesByMetadataName(entry.Type)
                .Select(type => new TypeDeclaration(type, entry.Member, parameterName: string.Empty)));

        var written = new[] { compilation.Assembly }
            .Concat(compilation.SourceModule.ReferencedAssemblySymbols)
            .SelectMany(assembly => assembly.GetAttributes())
            .Select(ReadAssemblyDeclaration)
            .OfType<TypeDeclaration>();

        return new IncludeDeclarations(builtIn.Concat(written));
    }

    /// <summary>
    /// Returns the name of the parameter a member hands its entities back from, and null when the member
    /// declares nothing. An empty name means the entities come from the value the member is used on.
    /// </summary>
    /// <param name="member">Method or property.</param>
    /// <returns>Parameter name, an empty string, or null.</returns>
    public string? FindSourceParameter(ISymbol member)
    {
        var definition = member.OriginalDefinition;

        // Written on the member itself.
        foreach (var attribute in definition.GetAttributes())
        {
            if (IsPreservesIncludes(attribute))
            {
                return GetStringArgument(attribute, 0) ?? string.Empty;
            }
        }

        // Written on an assembly or built in. An indexer is named "this[]" in C# and "Item" in metadata.
        var candidates = declarations[definition.Name].Concat(
            definition.MetadataName == definition.Name ? [] : declarations[definition.MetadataName]);

        return candidates.FirstOrDefault(declaration => declaration.Matches(definition))?.ParameterName;
    }

    private static IEnumerable<(string Type, string Member)> Declare(string type, IEnumerable<string> members)
        => members.Select(member => (type, member));

    private static TypeDeclaration? ReadAssemblyDeclaration(AttributeData attribute)
    {
        if (!IsPreservesIncludes(attribute) ||
            attribute.ConstructorArguments.Length < 2 ||
            attribute.ConstructorArguments[0].Value is not INamedTypeSymbol declaringType ||
            GetStringArgument(attribute, 1) is not { } memberName)
        {
            return null;
        }

        return new TypeDeclaration(declaringType, memberName, GetStringArgument(attribute, 2) ?? string.Empty);
    }

    private static bool IsPreservesIncludes(AttributeData attribute)
        => string.Equals(
            attribute.AttributeClass?.Name,
            nameof(PreservesIncludesAttribute),
            StringComparison.Ordinal);

    private static string? GetStringArgument(AttributeData attribute, int index)
        => attribute.ConstructorArguments.Length > index
            ? attribute.ConstructorArguments[index].Value as string
            : null;

    /// <summary>
    /// A declaration that names a member of a type: <c>[assembly: PreservesIncludes(typeof(SomeType), "Member")]</c>
    /// or a built-in one.
    /// </summary>
    private sealed class TypeDeclaration
    {
        private readonly INamedTypeSymbol declaringType;

        public TypeDeclaration(INamedTypeSymbol declaringType, string memberName, string parameterName)
        {
            this.declaringType = declaringType.OriginalDefinition;
            MemberName = memberName;
            ParameterName = parameterName;
        }

        public string MemberName { get; }

        public string ParameterName { get; }

        /// <summary>
        /// True if the member is declared by the declaring type, or by a type that derives from it or implements
        /// it. A name on an implementing type is enough: "foreach" over a List calls List's own public
        /// GetEnumerator, which is not the interface method, and still has to count as IEnumerable's.
        /// </summary>
        public bool Matches(ISymbol member)
        {
            if (member.ContainingType is not { } containingType)
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
    }
}
