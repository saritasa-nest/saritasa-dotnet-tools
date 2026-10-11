namespace Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

/// <summary>
/// Declares that the named method parameter must have the named navigation property loaded before the method is called.
/// Can be applied multiple times to cover multiple parameters or properties.
/// </summary>
/// <example>
/// Single requirement.
/// <code>
/// [IncludeRequired(nameof(user), nameof(User.Profile))]
/// void SetTimezone(User user, string timezone)
/// {
///     user.Profile.Timezone = timezone;
/// }
/// </code>
/// </example>
/// <example>
/// Stacked — both Profile and Address must be loaded.
/// <code>
/// [IncludeRequired(nameof(user), nameof(User.Profile))]
/// [IncludeRequired(nameof(user), nameof(User.Address))]
/// void UpdateUser(User user, UserDto dto) { ... }
/// </code>
/// </example>
/// <example>
/// Nested path, one segment per argument. <c>nameof(User.Settings.Profile)</c> gives only "Profile",
/// so it does not describe the path.
/// <code>
/// [IncludeRequired(nameof(user), nameof(User.Settings), nameof(Settings.Profile))]
/// void SetTimezone(User user, string timezone)
/// {
///     user.Settings.Profile.Timezone = timezone;
/// }
/// </code>
/// The same path as one dotted string: <c>[IncludeRequired(nameof(user), "Settings.Profile")]</c>.
/// </example>
/// <example>
/// A collection followed by a property of its elements, as <c>Include(...).ThenInclude(...)</c> loads it.
/// <code>
/// [IncludeRequired(nameof(admin), nameof(User.Applications), nameof(Application.Stats))]
/// void ExportApplicationStats(User admin)
/// {
///     foreach (var application in admin.Applications)
///     {
///         var stat = application.Stats;
///     }
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
    /// Navigation path that must be loaded before the method is called, from the parameter outwards,
    /// e.g. ["Settings", "Profile"] or ["Settings.Profile"]. Must match the property names exactly (case-sensitive).
    /// </summary>
    public string[] Path { get; }

    /// <summary>
    /// Initializes the attribute with the parameter name and the navigation path to require.
    /// </summary>
    /// <param name="param">Name of the method parameter (case-sensitive, must match exactly).</param>
    /// <param name="path">Navigation properties from the parameter outwards: one per argument, e.g. <c>nameof(User.Settings), nameof(Settings.Profile)</c>, or a dotted string such as "Settings.Profile" (case-sensitive, must match exactly).</param>
    public IncludeRequiredAttribute(string param, params string[] path)
    {
        Param = param;
        Path = path;
    }
}
