namespace Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

/// <summary>
/// Says that a method or a property hands back the entities it was given, so whatever was included on them is
/// still included on its result.
/// </summary>
/// <remarks>
/// The analyzer works this out on its own for most methods, by reading how they are declared: a method that
/// takes <c>IQueryable&lt;User&gt;</c> and returns <c>IQueryable&lt;User&gt;</c> clearly hands the same users
/// back. Use this attribute for the cases it cannot read, above all a library type that holds entities inside
/// without being a collection.
/// The attribute says nothing about which property is included. It only says where the entities come from,
/// and the analyzer follows them from there.
/// </remarks>
/// <example>
/// On your own method, naming the parameter the entities come from:
/// <code>
/// [PreservesIncludes(nameof(query))]
/// public PagedResult&lt;User&gt; Paginate(IQueryable&lt;User&gt; query, int page) { ... }
/// </code>
/// </example>
/// <example>
/// On your own property, meaning "my value holds the entities of the object I am on":
/// <code>
/// public class PagedResult&lt;T&gt;
/// {
///     [PreservesIncludes]
///     public List&lt;T&gt; Items { get; set; }
/// }
/// </code>
/// </example>
/// <example>
/// For a library you do not own, put it on your own assembly and name theirs:
/// <code>
/// [assembly: PreservesIncludes(typeof(SomeLib.QueryExtensions), "Paginate", "query")]
/// [assembly: PreservesIncludes(typeof(SomeLib.PagedResult&lt;&gt;), "Items")]
/// </code>
/// A declaration is picked up from the project it is written in and from every project and package that
/// references it, so a shared project can declare it once for a whole solution.
/// </example>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Assembly,
    AllowMultiple = true)]
public class PreservesIncludesAttribute : Attribute
{
    /// <summary>
    /// Declares that a property hands back the entities of the object it is on.
    /// </summary>
    public PreservesIncludesAttribute()
    {
    }

    /// <summary>
    /// Declares that a method hands back the entities of one of its parameters.
    /// </summary>
    /// <param name="parameterName">Name of the parameter the entities come from.</param>
    public PreservesIncludesAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    /// <summary>
    /// Declares that a property of another assembly hands back the entities of the object it is on.
    /// </summary>
    /// <param name="declaringType">Type that declares the property.</param>
    /// <param name="memberName">Name of the property.</param>
    public PreservesIncludesAttribute(Type declaringType, string memberName)
    {
        DeclaringType = declaringType;
        MemberName = memberName;
    }

    /// <summary>
    /// Declares that a method of another assembly hands back the entities of one of its parameters.
    /// </summary>
    /// <param name="declaringType">Type that declares the method.</param>
    /// <param name="memberName">Name of the method.</param>
    /// <param name="parameterName">Name of the parameter the entities come from.</param>
    public PreservesIncludesAttribute(Type declaringType, string memberName, string parameterName)
    {
        DeclaringType = declaringType;
        MemberName = memberName;
        ParameterName = parameterName;
    }

    /// <summary>
    /// Type that declares the member, when the attribute is put on an assembly to describe another one.
    /// Null when the attribute is put on the member itself.
    /// </summary>
    public Type? DeclaringType { get; }

    /// <summary>
    /// Name of the member being described, when the attribute is put on an assembly.
    /// Null when the attribute is put on the member itself.
    /// </summary>
    public string? MemberName { get; }

    /// <summary>
    /// Name of the parameter the entities come from. Null for a property, which hands back the entities of
    /// the object it is on.
    /// </summary>
    public string? ParameterName { get; }
}
