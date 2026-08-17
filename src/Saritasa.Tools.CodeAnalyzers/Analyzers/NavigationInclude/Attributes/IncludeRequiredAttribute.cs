namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

/// <summary>
/// Declares that the named method parameter must have the named navigation property loaded before the method is called.
/// Can be applied multiple times to cover multiple parameters or properties.
/// </summary>
/// <example>
/// Single requirement
/// <code>
/// [IncludeRequired(nameof(user), nameof(User.Profile))]
/// void SetTimezone(User user, string timezone)
/// {
///     user.Profile.Timezone = timezone;
/// }
/// </code>
/// </example>
/// <example>
/// Stacked — both Profile and Address must be loaded
/// <code>
/// [IncludeRequired(nameof(user), nameof(User.Profile))]
/// [IncludeRequired(nameof(user), nameof(User.Address))]
/// void UpdateUser(User user, UserDto dto) { ... }
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
    /// Initializes the attribute with the parameter name and the navigation property name to require.
    /// </summary>
    /// <param name="param">Name of the method parameter (case-sensitive, must match exactly).</param>
    /// <param name="includedProperty">Name of the navigation property that must be loaded (case-sensitive, must match exactly).</param>
    public IncludeRequiredAttribute(string param, string includedProperty)
    {
        Param = param;
        IncludedProperty = includedProperty;
    }
}
