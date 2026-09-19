using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Where the entities of an expression come from: a call, a property, an array element.
/// </summary>
/// <remarks>
/// The counterpart of <see cref="VariableOrigin"/>. Reading a variable is an expression too, but it is the one
/// case that cannot be answered here: a variable has to be traced back through the statements that ran before
/// it, which needs the search and its memory of the blocks already read. Every other expression answers from
/// itself alone and never moves the position.
/// A call or a property is followed only when it is declared with [PreservesIncludes], built in or written by
/// the project. To teach the search a new member, declare it rather than adding a rule here.
/// </remarks>
internal static class ExpressionOrigin
{
    /// <summary>
    /// Returns where the entities of the expression come from.
    /// </summary>
    /// <param name="origin">Origin whose value is read.</param>
    /// <param name="property">Navigation property the search is looking for.</param>
    /// <returns>Origin; <see cref="Origin.Missing"/> or <see cref="Origin.Unknown"/> when it cannot be followed.</returns>
    public static Origin GetOrigin(Origin origin, string property)
        => origin.Value switch
        {
            // "query.Include(u => u.Profile)", a call of a method declared with [Includes("Profile")].
            IInvocationOperation call when LoadsProperty(call, property)
                => Origin.Loaded,

            // A method declared with [PreservesIncludes]: "query.Where(...)", "users.ToList()",
            // "query.Paginate(1)". The declaration says where its entities come from.
            IInvocationOperation call when FindDeclaredSource(call, origin) is { } declared
                => Origin.Create(declared, origin.Position),

            // A call nobody declared. In our own code that is a real answer: the method promises nothing with
            // [Includes] or [PreservesIncludes], so nothing loads the property. In someone else's code we simply
            // cannot see.
            IInvocationOperation call
                => IsOurOwnCode(call.TargetMethod, origin) ? Origin.Missing : Origin.Unknown,

            // A property declared with [PreservesIncludes]: "users[0]", "enumerator.Current", "pair.Value",
            // "page.Items". It hands back entities of the object it is read on.
            IPropertyReferenceOperation reference when IsDeclaredPreserving(reference, origin)
                => Origin.Create(reference.Instance, origin.Position),

            // "users[0]" of an array. Not a property in Roslyn, so no declaration can describe it.
            IArrayElementReferenceOperation element
                => Origin.Create(element.ArrayReference, origin.Position),

            // "new User()": it was just made, so nothing is loaded on it.
            IObjectCreationOperation => Origin.Missing,

            // "dbContext.Users": the query starts here and no Include was put on it.
            IPropertyReferenceOperation reference when IsEntitySet(reference.Property.Type) => Origin.Missing,

            // A field, a call we could not follow, anything else: we cannot tell.
            _ => Origin.Unknown,
        };

    /// <summary>
    /// The argument a method declared with [PreservesIncludes] hands its entities back from, or null when the
    /// method declares nothing.
    /// </summary>
    private static IOperation? FindDeclaredSource(IInvocationOperation call, Origin origin)
    {
        var parameterName = origin.Position.FlowGraph.Declarations.FindSourceParameter(call.TargetMethod);

        // A declaration names a method, not one overload: "ToDictionary(u => u.Id, u => u.Name)" is declared
        // together with "ToDictionary(u => u.Id)", but it hands back names, not users.
        if (parameterName is null || !EntityFlow.HandsBackSource(call.TargetMethod, parameterName))
        {
            return null;
        }

        // [PreservesIncludes] with no argument: entities come from whatever the method is called on, e.g.
        // "query" in "query.Paginate(1)" (EntityFlow.GetSource finds it the same way for Where, ToList, ...).
        // [PreservesIncludes(nameof(query))]: entities come from the argument passed for that parameter, e.g.
        // "dbContext.Users" in "dbContext.Users.Paginate(1)".
        return parameterName.Length == 0
            ? EntityFlow.GetSource(call)
            : call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == parameterName)?.Value;
    }

    /// <summary>
    /// True if the property is declared with [PreservesIncludes], so it hands back the entities of the object
    /// it is on.
    /// </summary>
    private static bool IsDeclaredPreserving(IPropertyReferenceOperation reference, Origin origin)
        => origin.Position.FlowGraph.Declarations.FindSourceParameter(reference.Property) is { } parameterName &&
           EntityFlow.HandsBackSource(reference.Property, parameterName);

    /// <summary>
    /// True if the method is declared in the assembly being compiled, where the developer can read it and put
    /// [Includes] on it. A method from anywhere else cannot be read or annotated.
    /// </summary>
    private static bool IsOurOwnCode(IMethodSymbol method, Origin origin)
        => SymbolEqualityComparer.Default.Equals(
            method.ContainingAssembly,
            origin.Position.FlowGraph.Method.ContainingAssembly);

    /// <summary>
    /// True for the "DbSet&lt;User&gt;" of a context property, where a query starts.
    /// </summary>
    private static bool IsEntitySet(ITypeSymbol type)
        => type is INamedTypeSymbol named &&
           named.ConstructedFrom.MetadataName == "DbSet`1" &&
           named.ContainingNamespace?.ToDisplayString() == "Microsoft.EntityFrameworkCore";

    /// <summary>
    /// True if the call loads the property itself: <c>query.Include(u =&gt; u.Profile)</c> or a call of a method
    /// declared with <c>[Includes("Profile")]</c>.
    /// </summary>
    private static bool LoadsProperty(IInvocationOperation call, string property)
        => AttributeHelper.MethodHasIncludesAttribute(call.TargetMethod, property) ||
           EfIncludes.GetIncludedProperty(call) == property;
}
