namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

/// <summary>
/// Marks a navigation property as one that must be explicitly loaded (e.g. via
/// <c>.Include()</c>) before it is accessed. Applying this attribute triggers the
/// <c>NavigationIncludeAnalyzer</c>: any method that touches the property through a
/// parameter must declare <see cref="IncludeRequiredAttribute"/> for that parameter.
/// </summary>
/// <remarks>
/// Place this attribute on navigation properties of entity classes to enforce at
/// compile time that callers load them before use, preventing
/// </remarks>
/// <example>
/// <code>
/// public class User
/// {
///     public int Id { get; set; }
///
///     /// Navigation property — must be loaded explicitly before access.
///     [TrackIncludeRequired]
///     public UserProfile Profile { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class TrackIncludeRequiredAttribute : Attribute
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public TrackIncludeRequiredAttribute()
    {
    }
}
