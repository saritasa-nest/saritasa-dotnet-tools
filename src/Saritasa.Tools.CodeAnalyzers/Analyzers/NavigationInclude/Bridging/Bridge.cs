using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// One declared move of entities, from something a member hands back to one of the member's inputs.
/// </summary>
/// <remarks>
/// The search reads backwards, so a bridge is written that way too: the kind says what it runs from,
/// <see cref="Source"/> says where it lands. A member hands something back in three ways, so there are three
/// kinds, and one member can carry several — <c>Where</c> has both a <see cref="FromResult"/> and a
/// <see cref="FromLambdaParameter"/>.
/// </remarks>
internal abstract class Bridge
{
    private Bridge(string memberName, string source, string? notForOverloadWith)
    {
        MemberName = memberName;
        Source = source;
        NotForOverloadWith = notForOverloadWith;
    }

    /// <summary>
    /// Name of the method or property the bridge runs from.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Where the bridge lands: a parameter name, or empty for the value the member was used on.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Parameter whose presence means this overload hands back something else. Null when every overload
    /// behaves the same.
    /// </summary>
    /// <remarks>
    /// Counting parameters cannot separate the overloads — <c>Min(source, comparer)</c> hands back an element
    /// and <c>Min(source, selector)</c> does not, and both take two — so the odd one out is named. Renaming a
    /// public BCL parameter is a source-breaking change, so the name is safe to lean on.
    /// </remarks>
    public string? NotForOverloadWith { get; }

    /// <summary>
    /// True if the member is the overload this bridge does not describe.
    /// </summary>
    /// <param name="member">Member the search is standing on.</param>
    /// <returns>True when the bridge must not be used for it.</returns>
    public bool ExcludesOverloadOf(ISymbol member)
        => NotForOverloadWith is not null &&
           member is IMethodSymbol method &&
           method.Parameters.Any(parameter =>
               string.Equals(parameter.Name, NotForOverloadWith, StringComparison.Ordinal));

    /// <summary>
    /// From the value the member handed back: <c>users.ToList()</c>, <c>page.Items</c>, <c>users[0]</c>.
    /// </summary>
    public sealed class FromResult : Bridge
    {
        /// <summary>
        /// Initializes the bridge.
        /// </summary>
        /// <param name="memberName">Name of the method or property.</param>
        /// <param name="source">Where the bridge lands.</param>
        /// <param name="notForOverloadWith">Parameter of the overload this bridge does not describe.</param>
        public FromResult(string memberName, string source, string? notForOverloadWith = null)
            : base(memberName, source, notForOverloadWith)
        {
        }
    }

    /// <summary>
    /// From a parameter of a callback the member calls: the <c>u</c> of <c>users.Select(u =&gt; ...)</c>.
    /// </summary>
    public sealed class FromLambdaParameter : Bridge
    {
        /// <summary>
        /// Initializes the bridge.
        /// </summary>
        /// <param name="memberName">Name of the method.</param>
        /// <param name="source">Where the bridge lands.</param>
        /// <param name="callback">Parameter that takes the callback.</param>
        /// <param name="parameter">Position of the callback's own parameter.</param>
        public FromLambdaParameter(string memberName, string source, string callback, int parameter)
            : base(memberName, source, notForOverloadWith: null)
        {
            Callback = callback;
            Parameter = parameter;
        }

        /// <summary>
        /// Parameter that takes the callback.
        /// </summary>
        public string Callback { get; }

        /// <summary>
        /// Position of the callback's own parameter the bridge runs from.
        /// </summary>
        public int Parameter { get; }
    }

    /// <summary>
    /// From an out argument the member wrote: the <c>user</c> of
    /// <c>dictionary.TryGetValue(id, out var user)</c>.
    /// </summary>
    public sealed class FromOutArgument : Bridge
    {
        /// <summary>
        /// Initializes the bridge.
        /// </summary>
        /// <param name="memberName">Name of the method.</param>
        /// <param name="source">Where the bridge lands.</param>
        /// <param name="parameter">Name of the out parameter.</param>
        public FromOutArgument(string memberName, string source, string parameter)
            : base(memberName, source, notForOverloadWith: null)
        {
            Parameter = parameter;
        }

        /// <summary>
        /// Out parameter the bridge runs from.
        /// </summary>
        public string Parameter { get; }
    }
}
