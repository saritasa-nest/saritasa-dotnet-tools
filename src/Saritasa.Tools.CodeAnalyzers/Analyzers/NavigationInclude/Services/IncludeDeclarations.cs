using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// Every [PassesIncludes] the compilation can see, ready to look up by member.
/// </summary>
/// <remarks>
/// A declaration comes from one of three places, treated the same: the member itself, an
/// <c>[assembly: PassesIncludes]</c> in the project or anything it references, and the built-in list below.
/// Reading the references is the expensive part, so this is built once per compilation.
/// Nothing here is inferred. A member nobody declared is not followed, which is why
/// <see cref="BuiltInDeclarations"/> names every member of the framework the search is expected to cross.
/// </remarks>
internal sealed class IncludeDeclarations
{
    /// <summary>
    /// Nothing is declared anywhere. Used where a compilation is not available.
    /// </summary>
    public static readonly IncludeDeclarations None = new([]);

    /// <summary>
    /// Declarations written on assemblies or built in, grouped by member name so that a lookup only compares
    /// types for members with the right name.
    /// </summary>
    private readonly ILookup<string, TypeDeclaration> declarations;

    private IncludeDeclarations(IEnumerable<TypeDeclaration> declarations)
    {
        this.declarations = declarations.ToLookup(declaration => declaration.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// Reads every declaration the compilation can see.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>Declarations.</returns>
    public static IncludeDeclarations Read(Compilation compilation)
    {
        var builtIn = BuiltInDeclarations.All
            .SelectMany(entry => compilation
                .GetTypesByMetadataName(entry.Type)
                .Select(type => new TypeDeclaration(
                    type,
                    entry.Name,
                    entry.From,
                    entry.ToLambda,
                    entry.ToLambdaParameter,
                    entry.NotWhen,
                    isBuiltIn: true)));

        var written = new[] { compilation.Assembly }
            .Concat(compilation.SourceModule.ReferencedAssemblySymbols)
            .SelectMany(assembly => assembly.GetAttributes())
            .Select(ReadAssemblyDeclaration)
            .OfType<TypeDeclaration>();

        return new IncludeDeclarations(builtIn.Concat(written));
    }

    /// <summary>
    /// Returns the name of the parameter a method hands its entities back from, and null when the method
    /// declares nothing. An empty name means the entities come from the value the method is called on.
    /// </summary>
    /// <param name="method">Method being called.</param>
    /// <returns>Parameter name, an empty string, or null.</returns>
    public string? FindMethodSource(IMethodSymbol method)
    {
        var definition = method.OriginalDefinition;

        // Written on the method itself. A lambda declaration says nothing about the result, so it is skipped.
        foreach (var attribute in definition.GetAttributes())
        {
            if (IsDeclaration(attribute) && GetToLambda(attribute) is null)
            {
                return GetFrom(attribute);
            }
        }

        return FindInTable(definition, declaration => declaration.ToLambda is null)?.From;
    }

    /// <summary>
    /// Returns an empty string when a property hands back the entities of the object it is read on, and null
    /// when it does not.
    /// </summary>
    /// <remarks>
    /// Only the built-in lines describe a property. [PassesIncludes] cannot be written on one, and an
    /// assembly declaration naming one is ignored, so that every property bridge is one we can verify.
    /// </remarks>
    /// <param name="property">Property being read.</param>
    /// <returns>An empty string, or null.</returns>
    public string? FindPropertySource(IPropertySymbol property)
        => FindInTable(
            property.OriginalDefinition,
            declaration => declaration.ToLambda is null && declaration.IsBuiltIn)?.From;

    /// <summary>
    /// Returns the name of the parameter a lambda's own parameter is filled from, and null when nobody declared
    /// it. An empty name means the entities come from the value the member is used on.
    /// </summary>
    /// <param name="method">Method the lambda is passed to.</param>
    /// <param name="lambdaParameterName">Name of the method parameter that takes the lambda.</param>
    /// <param name="lambdaParameterOrdinal">Position of the lambda's own parameter.</param>
    /// <returns>Parameter name, an empty string, or null.</returns>
    public string? FindLambdaSource(
        IMethodSymbol method,
        string lambdaParameterName,
        int lambdaParameterOrdinal)
    {
        var definition = method.OriginalDefinition;

        foreach (var attribute in definition.GetAttributes())
        {
            if (IsDeclaration(attribute) &&
                string.Equals(GetToLambda(attribute), lambdaParameterName, StringComparison.Ordinal) &&
                GetToLambdaParameter(attribute) == lambdaParameterOrdinal)
            {
                return GetFrom(attribute);
            }
        }

        return FindInTable(
            definition,
            declaration =>
                string.Equals(declaration.ToLambda, lambdaParameterName, StringComparison.Ordinal) &&
                declaration.ToLambdaParameter == lambdaParameterOrdinal)?.From;
    }

    /// <summary>
    /// The first declaration of the right kind whose type matches the member.
    /// </summary>
    private TypeDeclaration? FindInTable(ISymbol definition, Func<TypeDeclaration, bool> isRightKind)
    {
        // An indexer is named "this[]" in C# and "Item" in metadata.
        var candidates = declarations[definition.Name].Concat(
            definition.MetadataName == definition.Name
                ? []
                : declarations[definition.MetadataName]);

        return candidates.FirstOrDefault(declaration => isRightKind(declaration) && declaration.Matches(definition));
    }

    private static TypeDeclaration? ReadAssemblyDeclaration(AttributeData attribute)
    {
        if (!IsDeclaration(attribute) ||
            attribute.ConstructorArguments.Length < 2 ||
            attribute.ConstructorArguments[0].Value is not INamedTypeSymbol declaringType ||
            GetStringArgument(attribute, 1) is not { } memberName)
        {
            return null;
        }

        return new TypeDeclaration(
            declaringType,
            memberName,
            GetStringArgument(attribute, 2) ?? string.Empty,
            GetToLambda(attribute),
            GetToLambdaParameter(attribute),
            notWhen: null,
            isBuiltIn: false);
    }

    private static bool IsDeclaration(AttributeData attribute)
        => string.Equals(
            attribute.AttributeClass?.Name,
            nameof(PassesIncludesAttribute),
            StringComparison.Ordinal);

    /// <summary>
    /// The parameter the entities come from, as written on the member itself. An empty string when the
    /// declaration names none, which means the value the member is used on.
    /// </summary>
    private static string GetFrom(AttributeData attribute)
        => GetStringArgument(attribute, 0) ?? string.Empty;

    private static string? GetToLambda(AttributeData attribute)
        => GetNamedArgument(attribute, nameof(PassesIncludesAttribute.ToLambda)).Value as string;

    private static int GetToLambdaParameter(AttributeData attribute)
        => GetNamedArgument(attribute, nameof(PassesIncludesAttribute.ToLambdaParameter)).Value is int ordinal
            ? ordinal
            : 0;

    private static TypedConstant GetNamedArgument(AttributeData attribute, string name)
        => attribute.NamedArguments
            .FirstOrDefault(argument => string.Equals(argument.Key, name, StringComparison.Ordinal))
            .Value;

    private static string? GetStringArgument(AttributeData attribute, int index)
        => attribute.ConstructorArguments.Length > index
            ? attribute.ConstructorArguments[index].Value as string
            : null;

    /// <summary>
    /// A declaration that names a member of a type: <c>[assembly: PassesIncludes(typeof(SomeType), "Member")]</c>
    /// or a built-in one.
    /// </summary>
    private sealed class TypeDeclaration
    {
        private readonly INamedTypeSymbol declaringType;
        private readonly string? notWhen;

        public TypeDeclaration(
            INamedTypeSymbol declaringType,
            string name,
            string from,
            string? toLambda,
            int toLambdaParameter,
            string? notWhen,
            bool isBuiltIn)
        {
            this.declaringType = declaringType.OriginalDefinition;
            Name = name;
            From = from;
            ToLambda = toLambda;
            ToLambdaParameter = toLambdaParameter;
            this.notWhen = notWhen;
            IsBuiltIn = isBuiltIn;
        }

        public string Name { get; }

        public string From { get; }

        /// <summary>
        /// Name of the parameter that takes the lambda, or null when the declaration describes the result.
        /// </summary>
        public string? ToLambda { get; }

        /// <summary>
        /// Position of the lambda's own parameter the entities arrive at.
        /// </summary>
        public int ToLambdaParameter { get; }

        /// <summary>
        /// True for a line that ships with the analyzer rather than one somebody wrote.
        /// </summary>
        public bool IsBuiltIn { get; }

        /// <summary>
        /// True if the symbol is declared by the declaring type, or by a type that derives from it or implements
        /// it. A name on an implementing type is enough: "foreach" over a List calls List's own public
        /// GetEnumerator, which is not the interface method, and still has to count as IEnumerable's.
        /// </summary>
        public bool Matches(ISymbol symbol)
        {
            if (symbol.ContainingType is not { } containingType)
            {
                return false;
            }

            // The overload that takes this parameter hands back something else, e.g. "Min(source, selector)".
            if (notWhen is not null &&
                symbol is IMethodSymbol method &&
                method.Parameters.Any(parameter =>
                    string.Equals(parameter.Name, notWhen, StringComparison.Ordinal)))
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
