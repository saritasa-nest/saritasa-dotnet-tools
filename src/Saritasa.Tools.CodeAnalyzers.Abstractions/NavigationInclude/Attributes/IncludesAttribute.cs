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
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class IncludesAttribute : Attribute
{
    /// <summary>
    /// Name of the navigation property that is guaranteed to be loaded on the return value.
    /// Must match the property name exactly (case-sensitive).
    /// </summary>
    public string IncludedProperty { get; }

    /// <summary>
    /// Whether the analyzer verifies that the method really loads the property (INCL003). Default is true.
    /// Set to false when the loading logic cannot be followed by the analyzer; callers still rely on the promise.
    /// </summary>
    public bool Verify { get; set; } = true;

    /// <summary>
    /// Initializes the attribute with the name of the navigation property guaranteed to be loaded.
    /// </summary>
    /// <param name="includedProperty">Name of the navigation property loaded on the return value (case-sensitive, must match exactly).</param>
    public IncludesAttribute(string includedProperty)
    {
        IncludedProperty = includedProperty;
    }
}
