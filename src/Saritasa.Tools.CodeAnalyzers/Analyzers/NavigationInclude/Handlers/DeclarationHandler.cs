using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Handlers;

/// <summary>
/// Reports INCL005 when an <c>[assembly: PassesIncludes]</c> names a method or a parameter that does not exist.
/// </summary>
/// <remarks>
/// Declarations are the only way the search learns that a method passes entities on, so a broken one does not
/// fail loudly: the method is simply not followed any more. That happens quietly after a library renames a
/// method, which is why it is reported. A declaration written on the method itself cannot go wrong this way,
/// since it names its parameter with nameof.
/// </remarks>
internal static class DeclarationHandler
{
    /// <summary>
    /// Checks one attribute.
    /// </summary>
    /// <param name="context">Syntax node analysis context.</param>
    public static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var attribute = (AttributeSyntax)context.Node;

        if (!IsOnAssembly(attribute) ||
            !IsPassesIncludes(attribute, context) ||
            ReadNamedMethod(attribute, context) is not { } named ||
            NamedMethodExists(named.Type, named.Method, named.Parameter))
        {
            return;
        }

        var description = named.Type.ToDisplayString() + "." + named.Method +
                          (string.IsNullOrEmpty(named.Parameter) ? string.Empty : "(" + named.Parameter + ")");

        context.ReportDiagnostic(Diagnostic.Create(
            NavigationIncludeRulesProvider.GetDiagnosticDescriptor(
                NavigationIncludeRulesProvider.Incl5IdDeclarationNamesNothing),
            attribute.GetLocation(),
            description));
    }

    /// <summary>
    /// True for an attribute written as <c>[assembly: ...]</c>. Only those name someone else's method by string.
    /// </summary>
    private static bool IsOnAssembly(AttributeSyntax attribute)
        => attribute.Parent is AttributeListSyntax { Target.Identifier.RawKind: (int)SyntaxKind.AssemblyKeyword };

    /// <summary>
    /// True if the attribute is [PassesIncludes], and not some other assembly attribute.
    /// </summary>
    private static bool IsPassesIncludes(AttributeSyntax attribute, SyntaxNodeAnalysisContext context)
        => context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol is IMethodSymbol constructor &&
           constructor.ContainingType.Name == nameof(PassesIncludesAttribute);

    /// <summary>
    /// Reads what the declaration names, e.g. (QueryExtensions, "Paginate", "query") from
    /// <c>[assembly: PassesIncludes(typeof(QueryExtensions), "Paginate", "query")]</c>. Null when the attribute
    /// is not written in that form, for example while it is still being typed.
    /// </summary>
    private static (INamedTypeSymbol Type, string Method, string? Parameter)? ReadNamedMethod(
        AttributeSyntax attribute,
        SyntaxNodeAnalysisContext context)
    {
        var arguments = attribute.ArgumentList?.Arguments ?? default;
        if (arguments.Count < 2)
        {
            return null;
        }

        var type = GetTypeOfArgument(context, arguments[0]);
        var method = GetString(context, arguments[1]);
        if (type is null || method is null)
        {
            return null;
        }

        var parameter = arguments.Count > 2 ? GetString(context, arguments[2]) : null;

        return (type, method, parameter);
    }

    /// <summary>
    /// The type in an argument written as <c>typeof(QueryExtensions)</c>; null for any other argument.
    /// </summary>
    private static INamedTypeSymbol? GetTypeOfArgument(SyntaxNodeAnalysisContext context, AttributeArgumentSyntax argument)
        => argument.Expression is TypeOfExpressionSyntax typeOf
            ? context.SemanticModel.GetTypeInfo(typeOf.Type, context.CancellationToken).Type as INamedTypeSymbol
            : null;

    /// <summary>
    /// True if the type has the method and, when a parameter is named, a method with that name has it.
    /// </summary>
    private static bool NamedMethodExists(INamedTypeSymbol type, string method, string? parameter)
    {
        var candidates = FindMethods(type, method).ToList();

        if (string.IsNullOrEmpty(parameter))
        {
            return candidates.Count > 0;
        }

        return candidates.Any(candidate =>
            candidate.Parameters.Any(parameter1 => parameter1.Name == parameter));
    }

    /// <summary>
    /// Methods with the name on the type, on its base types and on its interfaces, the same places a declaration
    /// is matched against. Only methods, because a declaration cannot describe a property.
    /// </summary>
    private static IEnumerable<IMethodSymbol> FindMethods(INamedTypeSymbol type, string methodName)
    {
        // "typeof(PagedResult<>)" is an unbound generic type, which has no members of its own; its definition has.
        type = type.OriginalDefinition;

        var types = new List<INamedTypeSymbol>();
        for (var current = type; current is not null; current = current.BaseType)
        {
            types.Add(current);
        }

        types.AddRange(type.AllInterfaces);

        return types
            .SelectMany(candidate => candidate.GetMembers())
            .OfType<IMethodSymbol>()
            .Where(candidate => candidate.Name == methodName || candidate.MetadataName == methodName);
    }

    private static string? GetString(SyntaxNodeAnalysisContext context, AttributeArgumentSyntax argument)
        => context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken).Value as string;
}
