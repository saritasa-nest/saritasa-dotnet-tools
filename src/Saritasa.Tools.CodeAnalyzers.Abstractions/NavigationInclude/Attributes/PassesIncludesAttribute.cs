namespace Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

/// <summary>
/// Declares where a method's entities come from, so the analyzer can follow an <c>.Include()</c> across it.
/// </summary>
/// <remarks>
/// The analyzer reads backwards from where a navigation property is used to the query that loaded it, and it
/// does not read method bodies. So at every call it has to be told whether the entities coming out are the
/// ones that went in; without that the trail stops and you get INCL004, "cannot check".
/// It says nothing about which property is loaded, only where the entities travelled, which is why one
/// attribute covers every property. <see cref="From"/> names the value holding them, and they arrive at the
/// result, or at a callback parameter when <see cref="ToCallback"/> is set.
/// </remarks>
/// <example>
/// The result holds the entities of a parameter:
/// <code>
/// [PassesIncludes(nameof(source))]
/// public static PagedList&lt;T&gt; FromSource&lt;T&gt;(IQueryable&lt;T&gt; source, int page) { ... }
/// </code>
/// </example>
/// <example>
/// The result holds the entities of the object the method is called on:
/// <code>
/// [PassesIncludes]
/// public PagedList&lt;User&gt; TakePage(int page) { ... }
/// </code>
/// </example>
/// <example>
/// A callback the method calls is handed elements of the object it is called on:
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
/// references it, so a shared project can declare a bridge once for a whole solution.
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = true)]
public class PassesIncludesAttribute : Attribute
{
    /// <summary>
    /// Declares that the entities come from the object the method is called on.
    /// </summary>
    public PassesIncludesAttribute()
    {
    }

    /// <summary>
    /// Declares that the entities come from one of the method's parameters.
    /// </summary>
    /// <param name="from">Name of the parameter the entities come from.</param>
    public PassesIncludesAttribute(string from)
    {
        From = from;
    }

    /// <summary>
    /// Declares that the entities of a method of another assembly come from the object it is called on.
    /// </summary>
    /// <param name="declaringType">Type that declares the method.</param>
    /// <param name="methodName">Name of the method.</param>
    public PassesIncludesAttribute(Type declaringType, string methodName)
    {
        DeclaringType = declaringType;
        MethodName = methodName;
    }

    /// <summary>
    /// Declares that the entities of a method of another assembly come from one of its parameters.
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
    /// Name of the parameter the entities come from. Null when they come from the object the method is
    /// called on.
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
    public int ToCallbackParameter { get; set; }
}
