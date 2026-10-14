using System.Collections.Immutable;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

/// <summary>
/// A chain of navigation properties read from an entity, e.g. "Settings.Profile" of a user.
/// </summary>
/// <remarks>
/// <c>nameof(User.Settings.Profile)</c> gives only "Profile", so a path is declared segment by segment:
/// <c>[IncludeRequired(nameof(user), nameof(User.Settings), nameof(Settings.Profile))]</c>, or as one dotted string
/// <c>"Settings.Profile"</c>. Both are read into the same path.
/// </remarks>
internal sealed class NavigationPath : IEquatable<NavigationPath>
{
    private NavigationPath(ImmutableArray<string> segments)
    {
        Segments = segments;
    }

    /// <summary>
    /// Property names from the entity outwards, e.g. ["Settings", "Profile"].
    /// </summary>
    public ImmutableArray<string> Segments { get; }

    /// <summary>
    /// Reads a path from attribute arguments or an include string. Each argument may hold several segments
    /// separated by dots.
    /// </summary>
    /// <param name="arguments">Arguments, e.g. ["Settings", "Profile"] or ["Settings.Profile"].</param>
    /// <returns>Path, or null when there are no segments or one of them is empty.</returns>
    public static NavigationPath? Parse(IEnumerable<string?> arguments)
    {
        var segments = ImmutableArray.CreateBuilder<string>();

        foreach (var argument in arguments)
        {
            if (argument is null)
            {
                return null;
            }

            foreach (var segment in argument.Split('.'))
            {
                // "Settings..Profile" or "Settings." is a typo, not a shorter path.
                if (string.IsNullOrWhiteSpace(segment))
                {
                    return null;
                }

                segments.Add(segment);
            }
        }

        return segments.Count == 0
            ? null
            : new NavigationPath(segments.ToImmutable());
    }

    /// <summary>
    /// Returns the path that reaches this one through the segments, e.g. "Settings" before "Profile" gives
    /// "Settings.Profile".
    /// </summary>
    /// <param name="segments">Segments to put in front.</param>
    /// <returns>Longer path.</returns>
    public NavigationPath Prepend(params string[] segments)
        => new(Segments.InsertRange(0, segments));

    /// <summary>
    /// True if loading this path also loads the required one: "Settings.Profile" covers "Settings" and itself.
    /// </summary>
    /// <param name="required">Required path.</param>
    /// <returns>True if the required path is a prefix of this one.</returns>
    public bool Covers(NavigationPath required)
    {
        if (required.Segments.Length > Segments.Length)
        {
            return false;
        }

        for (var i = 0; i < required.Segments.Length; i++)
        {
            if (!string.Equals(Segments[i], required.Segments[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// True if both paths have the same segments. The walker's memory of read blocks compares paths this way.
    /// </summary>
    /// <param name="other">Other path.</param>
    /// <returns>True if equal.</returns>
    public bool Equals(NavigationPath? other)
        => other is not null && Segments.SequenceEqual(other.Segments, StringComparer.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as NavigationPath);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Combines the segment hashes in order, so "A.B" and "B.A" differ. System.HashCode is not available on
        // .NET Standard 2.0; 17 and 31 are the usual odd primes for this.
        var hash = 17;

        foreach (var segment in Segments)
        {
            hash = unchecked((hash * 31) + StringComparer.Ordinal.GetHashCode(segment));
        }

        return hash;
    }

    /// <summary>
    /// Returns the dotted form, e.g. "Settings.Profile", as messages and raw-string attributes use it.
    /// </summary>
    /// <returns>Dotted path.</returns>
    public override string ToString() => string.Join(".", Segments);
}
