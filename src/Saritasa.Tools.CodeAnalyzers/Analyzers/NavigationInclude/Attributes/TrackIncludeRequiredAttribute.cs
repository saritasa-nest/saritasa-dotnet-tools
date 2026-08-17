namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

/// <summary>
/// Marks a navigation property as requiring explicit loading via <c>.Include()</c>.
/// Methods that access it must declare <c>[IncludeRequired]</c>.
/// </summary>
/// <example>
/// Only properties that need tracking should be annotated — untracked properties produce no diagnostics:
/// <code>
/// class User
/// {
///     public Organization Organization { get; set; } // not tracked
///
///     [TrackIncludeRequired]
///     public UserProfile Profile { get; set; } // tracked — callers must ensure it is loaded
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
