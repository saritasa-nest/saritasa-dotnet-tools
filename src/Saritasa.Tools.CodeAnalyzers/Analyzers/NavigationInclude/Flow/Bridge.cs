using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// A move from one value to another that keeps the same entities: <c>query.Where(...)</c>,
/// <c>users.ToList()</c>, <c>users[0]</c>, <c>page.Items</c>, <c>query.Paginate(1)</c>.
/// </summary>
/// <remarks>
/// The search asks one thing here: may the entities of this place have been somewhere else before, and where?
/// The shape of the move does not matter. A collection can become another collection (<c>Where</c>), a
/// collection can become one element (<c>First</c>, <c>users[0]</c>), and one value can become a collection
/// (<c>page.Items</c>). What matters is that the entities on both sides are the same ones.
/// A call moves entities in two directions, and both are the same bridge read from a different end: out of the
/// call into its result (<c>users.ToList()</c>), and into the call's own lambda
/// (<c>users.Select(u =&gt; ...)</c>, where <c>u</c> is filled from <c>users</c>). One
/// <see cref="FindSource(Value)"/> answers the first, the other answers the second.
/// Both ends need a declaration, written on the method, on an assembly or built in. Nothing is inferred from a
/// signature or a type: what nobody declared is not crossed, and an overload that hands back something else is
/// excluded by the name of its parameter where it is declared, not worked out here.
/// </remarks>
internal static class Bridge
{
    /// <summary>
    /// The value the entities came from, or null when the move out of this value is not declared or there is
    /// nothing on the other side of it.
    /// </summary>
    /// <param name="value">Value the search is reading.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FindSource(Value value)
        => value.Operation switch
        {
            // "query.Where(...)", "users.ToList()", "query.Paginate(1)". Returns query/users
            IInvocationOperation call
                when value.Position.FlowGraph.Declarations.FindMethodSource(call.TargetMethod) is { } from
                => GetCallSource(call, from),

            // "users[0]", "enumerator.Current", "pair.Value", "page.Items": the entities come from the object
            // the property is read on. Returns users/enumerator/pair/page
            IPropertyReferenceOperation reference
                when value.Position.FlowGraph.Declarations.FindPropertySource(reference.Property) is not null
                => reference.Instance,

            // "users[0]" of an array. Roslyn exposes nothing to name here, so no declaration can describe
            // it, and there is only one value the element can come from. Returns users
            IArrayElementReferenceOperation element
                => element.ArrayReference,

            _ => null,
        };

    /// <summary>
    /// The collection a lambda parameter is filled from: for the <c>u</c> of <c>users.Select(u =&gt; ...)</c>,
    /// the value <c>users</c>. Null when nobody declared that this parameter is handed an element, as for the
    /// index of <c>Select((u, i) =&gt; ...)</c> or the key of <c>GroupBy</c>, or when the lambda is not passed
    /// to a call at all.
    /// </summary>
    /// <param name="lambda">Lambda the parameter belongs to.</param>
    /// <param name="parameterOrdinal">Position of the parameter in the lambda.</param>
    /// <param name="declarations">Every [PassesIncludes] the compilation can see.</param>
    /// <returns>Source or null.</returns>
    public static IOperation? FindSource(
        IFlowAnonymousFunctionOperation lambda,
        int parameterOrdinal,
        IncludeDeclarations declarations)
    {
        var parent = lambda.Parent;
        while (parent is IConversionOperation or IDelegateCreationOperation)
        {
            parent = parent.Parent;
        }

        if (parent is not IArgumentOperation { Parameter: { } lambdaArgument, Parent: IInvocationOperation call })
        {
            return null;
        }

        // The declaration names the callback and the position inside it, so no overload check is needed: the
        // return type of the call says nothing about what the callback is handed.
        return declarations.FindLambdaSource(call.TargetMethod, lambdaArgument.Name, parameterOrdinal) is { } from
            ? GetCallSource(call, from)
            : null;
    }

    /// <summary>
    /// The value a call hands its entities back from.
    /// </summary>
    /// <remarks>
    /// A declaration with no parameter named: the entities come from whatever the method is called on, e.g.
    /// "query" in "query.Paginate(1)", the same way as for Where, ToList and the rest.
    /// A declaration naming a parameter: they come from the argument passed for it, e.g. "dbContext.Users" in
    /// "dbContext.Users.Paginate(1)".
    /// </remarks>
    private static IOperation? GetCallSource(IInvocationOperation call, string parameterName)
        => parameterName.Length == 0
            ? RoslynHelper.GetReceiver(call)
            : call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == parameterName)?.Value;
}
