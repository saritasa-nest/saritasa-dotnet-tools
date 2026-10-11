namespace Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

/// <summary>
/// Promises that the method's return value has the named navigation property loaded.
/// Can be applied multiple times to cover multiple properties.
/// </summary>
/// <example>
/// <code>
/// [Includes(nameof(User.Profile)]
/// async Task&lt;User&gt; GetUser(int id)
/// {
///     return await _dbContext.Users
///         .Include(u =&gt; u.Profile)
///         .FirstOrDefaultAsync(u =&gt; u.Id == id);
/// }
///
/// // Callers can now use GetUser without INCL002 warnings
/// async Task Handle(SaveUserDto dto)
/// {
///     var user = await GetUser(dto.Id);
///     SetTimezone(user, dto.Timezone);  // no warning
/// }
/// </code>
/// </example>
/// <example>
/// Verification disabled: the analyzer trusts the promise without checking the method body.
/// <code>
/// [Includes(nameof(User.Profile), Verify = false)]
/// Task&lt;User&gt; GetUser(int id) => _repository.GetWithIncludesAsync(id, IncludeProfile);
/// </code>
/// </example>
/// <example>
/// Nested path, one segment per argument, or as one dotted string <c>[Includes("Settings.Profile")]</c>.
/// <code>
/// [Includes(nameof(User.Settings), nameof(Settings.Profile))]
/// async Task&lt;User&gt; GetUser(int id)
/// {
///     return await _dbContext.Users
///         .Include(u =&gt; u.Settings)
///         .ThenInclude(s =&gt; s.Profile)
///         .FirstOrDefaultAsync(u =&gt; u.Id == id);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class IncludesAttribute : Attribute
{
    /// <summary>
    /// Navigation path that is guaranteed to be loaded on the return value, from the returned entity outwards,
    /// e.g. ["Settings", "Profile"] or ["Settings.Profile"]. Must match the property names exactly (case-sensitive).
    /// </summary>
    public string[] Path { get; }

    /// <summary>
    /// Whether the analyzer verifies that the method really loads the property (INCL003). Default is true.
    /// Set to false when the loading logic cannot be followed by the analyzer; callers still rely on the promise.
    /// </summary>
    public bool Verify { get; set; } = true;

    /// <summary>
    /// Initializes the attribute with the navigation path guaranteed to be loaded.
    /// </summary>
    /// <param name="path">Navigation properties from the returned entity outwards: one per argument, e.g. <c>nameof(User.Settings), nameof(Settings.Profile)</c>, or a dotted string such as "Settings.Profile" (case-sensitive, must match exactly).</param>
    public IncludesAttribute(params string[] path)
    {
        Path = path;
    }
}
