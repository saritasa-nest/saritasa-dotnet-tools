namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

/// <summary>
/// Declares that a method requires a specific navigation property to be loaded for one
/// of its parameters. Paired with <see cref="TrackIncludeRequiredAttribute"/> on the
/// property: any method that receives an entity with such a property and accesses it
/// (directly or by passing the parameter to a callee) must carry this attribute.
/// </summary>
/// <remarks>
/// The analyzer uses this attribute to propagate the include requirement up the call
/// chain (INCL001) and to verify that local variables are properly loaded before being
/// passed to such methods (INCL002).
/// </remarks>
/// <example>
/// <code>
/// [IncludeRequired(nameof(user), nameof(User.Profile))]
/// public void SetTimezone(User user, string timezone)
/// {
///     user.Profile.Timezone = timezone;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class IncludeRequiredAttribute : Attribute
{
    /// <summary>
    /// Name of the method parameter whose navigation property must be loaded.
    /// Must match the parameter name exactly (case-sensitive).
    /// </summary>
    public string Param { get; set; }

    /// <summary>
    /// Name of the navigation property that must be loaded before the method is called.
    /// Must match the property name exactly (case-sensitive).
    /// </summary>
    public string IncludedProperty { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="param">Name of parameter.</param>
    /// <param name="includedProperty">Included property.</param>
    public IncludeRequiredAttribute(string param, string includedProperty)
    {
        Param = param;
        IncludedProperty = includedProperty;
    }
}
