using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;

/// <summary>
/// Reports what is wrong with a <c>[PassesIncludes]</c> where it was written: INCL005 when it names a method
/// or a parameter that does not exist, INCL006 when the method is one it may not describe.
/// </summary>
/// <remarks>
/// Neither fails loudly on its own: the method is simply not followed any more, and the effect turns up as
/// an INCL004 in a different file. INCL005 is only for the assembly form, which names things by string;
/// INCL006 applies to both.
/// </remarks>
internal static class BridgeAttributeHandler
{
    /// <summary>
    /// Checks one attribute.
    /// </summary>
    /// <param name="context">Syntax node analysis context.</param>
    public static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var attribute = (AttributeSyntax)context.Node;

        if (!IsPassesIncludes(attribute, context))
        {
            return;
        }

        if (IsOnAssembly(attribute))
        {
            AnalyzeOnAssembly(context, attribute);
        }
        else
        {
            AnalyzeOnMethod(context, attribute);
        }
    }

    /// <summary>
    /// An <c>[assembly: PassesIncludes(typeof(Other), "Member", "source")]</c>: it may name nothing, or name
    /// a method it may not describe.
    /// </summary>
    private static void AnalyzeOnAssembly(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
    {
        if (ReadNamedMethod(attribute, context) is not { } named)
        {
            return;
        }

        var description = named.Type.ToDisplayString() + "." + named.Method +
                          (string.IsNullOrEmpty(named.Parameter) ? string.Empty : "(" + named.Parameter + ")");

        if (!NamedMethodExists(named.Type, named.Method, named.Parameter))
        {
            Report(context, attribute, NavigationIncludeRulesProvider.Incl5IdBridgeNamesNothing, description);

            return;
        }

        if (FindAttributeData(context.Compilation.Assembly, attribute) is not { } data ||
            PassesIncludesReader.ReadAssemblyAttribute(data) is not { } declared)
        {
            return;
        }

        // The attribute names a member by string, so it speaks for every overload of it. It is worth
        // reporting only when not one of them is a method the rules allow.
        var methods = FindMethods(named.Type, named.Method).ToList();

        if (methods.Count == 0 ||
            methods.Any(method => CustomBridgeRules.IsAllowed(method, declared.Bridge, out _)))
        {
            return;
        }

        CustomBridgeRules.IsAllowed(methods[0], declared.Bridge, out var reason);
        Report(context, attribute, NavigationIncludeRulesProvider.Incl6IdBridgeIsNotAllowed, description, reason);
    }

    /// <summary>
    /// A <c>[PassesIncludes]</c> on the method itself. It cannot name the wrong thing, but the method may
    /// still be one the rules do not allow.
    /// </summary>
    private static void AnalyzeOnMethod(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
    {
        if (attribute.Parent?.Parent is not MethodDeclarationSyntax declaration ||
            context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not { } method ||
            FindAttributeData(method, attribute) is not { } data ||
            PassesIncludesReader.ReadMemberAttribute(data) is not { } bridge ||
            CustomBridgeRules.IsAllowed(method, bridge, out var reason))
        {
            return;
        }

        Report(
            context,
            attribute,
            NavigationIncludeRulesProvider.Incl6IdBridgeIsNotAllowed,
            method.ContainingType.ToDisplayString() + "." + method.Name,
            reason);
    }

    /// <summary>
    /// What the compiler made of this attribute, so the rules read it exactly as the lookup does.
    /// </summary>
    private static AttributeData? FindAttributeData(ISymbol owner, AttributeSyntax attribute)
        => owner.GetAttributes()
            .FirstOrDefault(data =>
                data.ApplicationSyntaxReference is { } reference &&
                reference.Span == attribute.Span &&
                reference.SyntaxTree == attribute.SyntaxTree);

    private static void Report(
        SyntaxNodeAnalysisContext context,
        AttributeSyntax attribute,
        string ruleId,
        params object[] arguments)
        => context.ReportDiagnostic(Diagnostic.Create(
            NavigationIncludeRulesProvider.GetDiagnosticDescriptor(ruleId),
            attribute.GetLocation(),
            arguments));

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
    /// What the declaration names, e.g. (QueryExtensions, "Paginate", "query"). Null while the attribute is
    /// still being typed.
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
    /// Methods with the name on the type, its base types and its interfaces: the places a bridge is matched
    /// against. Only methods, since the attribute cannot describe a property.
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
