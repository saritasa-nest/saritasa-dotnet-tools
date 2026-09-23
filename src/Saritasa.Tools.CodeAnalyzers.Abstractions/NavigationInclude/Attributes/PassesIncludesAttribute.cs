namespace Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

/// <summary>
/// States where a method's entities come from, so the analyzer can follow an <c>.Include()</c> across it.
/// </summary>
/// <remarks>
/// The analyzer reads backwards from where a navigation property is used to the query that loaded it, and it
/// does not read method bodies. So at every call it has to be told whether the entities coming out are the
/// ones that went in; without that the trail stops and you get INCL004, "cannot check".
/// It says nothing about which property is loaded, only where the entities travelled, which is why one
/// attribute covers every property. <see cref="From"/> names the value holding them, and they arrive at the
/// result, or at a callback parameter when <see cref="ToCallback"/> is set.
/// <para>
/// <b>What you are promising.</b> That the same entity <i>objects</i> come out that went in — not merely
/// objects of the same type. Nothing here can check that, so a wrong annotation does not fail loudly: the
/// analyzer believes it and stops reporting real mistakes. A method that reads a navigation property and
/// hands back what it found, such as one returning <c>users.SelectMany(u =&gt; u.Friends)</c>, is a different
/// set of objects with different includes and must not be annotated.
/// </para>
/// <para>
/// <b>Two rules, both reported as INCL006 and otherwise ignored.</b>
/// </para>
/// <list type="number">
/// <item><description>
/// The method must be <b>static</b>. An instance method can change what it was called on, or hand back
/// something built from a field, and the annotation would look the same. An extension method counts as
/// static, and the value in front of the dot is simply its first parameter.
/// </description></item>
/// <item><description>
/// The <b>entity type must not change</b>. The container may: <c>IQueryable&lt;User&gt;</c> to
/// <c>List&lt;User&gt;</c>, to <c>Task&lt;User&gt;</c>, to a single <c>User</c>. A container of your own
/// counts only if the analyzer can read it, which means implementing <c>IEnumerable&lt;T&gt;</c>. Otherwise
/// it cannot be told apart from a bridge to an unrelated entity.
/// </description></item>
/// </list>
/// </remarks>
/// <example>
/// The result holds the entities of a parameter:
/// <code>
/// [PassesIncludes(nameof(source))]
/// public static PagedList&lt;T&gt; FromSource&lt;T&gt;(IQueryable&lt;T&gt; source, int page) { ... }
/// </code>
/// </example>
/// <example>
/// For an extension method, naming no parameter means the value in front of the dot:
/// <code>
/// [PassesIncludes]
/// public static IQueryable&lt;User&gt; OnlyActive(this IQueryable&lt;User&gt; query) { ... }
/// </code>
/// </example>
/// <example>
/// A callback the method calls is handed elements of the collection:
/// <code>
/// [PassesIncludes(ToCallback = nameof(action))]
/// public static void ForEachItem&lt;T&gt;(this IEnumerable&lt;T&gt; items, Action&lt;T&gt; action) { ... }
/// </code>
/// </example>
/// <example>
/// For a library you do not own, put it on your own assembly and name theirs:
/// <code>
/// [assembly: PassesIncludes(typeof(SomeLib.QueryExtensions), "Paginate", "query")]
/// </code>
/// The attribute is picked up from the project it is written in and from every project and package that
/// references it, so a shared project can annotate a member once for a whole solution. It names the method by
/// string, so it speaks for every overload of that name at once.
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = true)]
public class PassesIncludesAttribute : Attribute
{
    /// <summary>
    /// States that the entities come from the value the extension method was used on, which is its first
    /// parameter.
    /// </summary>
    public PassesIncludesAttribute()
    {
    }

    /// <summary>
    /// States that the entities come from one of the method's parameters.
    /// </summary>
    /// <param name="from">Name of the parameter the entities come from.</param>
    public PassesIncludesAttribute(string from)
    {
        From = from;
    }

    /// <summary>
    /// States that the entities of a method of another assembly come from the value it was used on.
    /// </summary>
    /// <param name="declaringType">Type that declares the method.</param>
    /// <param name="methodName">Name of the method.</param>
    public PassesIncludesAttribute(Type declaringType, string methodName)
    {
        DeclaringType = declaringType;
        MethodName = methodName;
    }

    /// <summary>
    /// States that the entities of a method of another assembly come from one of its parameters.
    /// </summary>
    /// <param name="declaringType">Type that declares the method.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="from">Name of the parameter the entities come from.</param>
    public PassesIncludesAttribute(Type declaringType, string methodName, string from)
    {
        DeclaringType = declaringType;
        MethodName = methodName;
        From = from;
    }

    /// <summary>
    /// Type that declares the method, when the attribute is put on an assembly to describe another one.
    /// Null when the attribute is put on the method itself.
    /// </summary>
    public Type? DeclaringType { get; }

    /// <summary>
    /// Name of the method being described, when the attribute is put on an assembly.
    /// Null when the attribute is put on the method itself.
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    /// Name of the parameter the entities come from. Null when they come from the value the method was used
    /// on, which for an extension method is its first parameter.
    /// </summary>
    public string? From { get; }

    /// <summary>
    /// Name of the parameter that takes the callback, when the entities arrive at what that callback is called
    /// with instead of at what the method returns. Null when they arrive at the result.
    /// </summary>
    public string? ToCallback { get; set; }

    /// <summary>
    /// Position of the callback's own parameter the entities arrive at. Zero by default, which is the element
    /// parameter of every single-parameter callback such as <c>Action&lt;T&gt;</c>.
    /// </summary>
    /// <remarks>
    /// The position has to be written down rather than worked out, because a callback is handed values from
    /// more than one place and their types do not tell them apart: the index of
    /// <c>Select((u, i) =&gt; ...)</c> is not an element, and neither is the accumulator of
    /// <c>Aggregate(seed, (acc, u) =&gt; ...)</c>, which has the element's type.
    /// </remarks>
    public int ToCallbackParameter { get; set; }
}
