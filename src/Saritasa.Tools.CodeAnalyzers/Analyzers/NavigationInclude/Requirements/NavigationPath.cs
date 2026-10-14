using System.Collections.Immutable;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

/// <summary>
/// A chain of navigation properties read from an entity, e.g. "Settings.Profile" of a user.
/// </summary>
/// <remarks>
/// <c>nameof(User.Settings.Profile)</c> gives only "Profile", so a path is declared segment by segment:
/// <c>[IncludeRequired(nameof(user), nameof(User.Settings), nameof(Settings.Profile))]</c>, or as one dotted string
/// <c>"Settings.Profile"</c>. Both are read into the same path.
/// <para>
/// While the search walks back through <c>Include(...).ThenInclude(...)</c>, the path also holds a cursor,
/// <see cref="Matched"/>: how many segments at its end ThenIncludes have matched so far. The segments themselves
/// never shrink, so every answer still names the whole path.
/// </para>
/// </remarks>
internal sealed class NavigationPath : IEquatable<NavigationPath>
{
    private NavigationPath(ImmutableArray<string> segments, int matched = 0)
    {
        Segments = segments;
        Matched = matched;
    }

    /// <summary>
    /// Property names from the entity outwards, e.g. ["Settings", "Profile"].
    /// </summary>
    public ImmutableArray<string> Segments { get; }

    /// <summary>
    /// How many segments at the end ThenIncludes have matched: 1 for "Settings.Profile" after
    /// <c>ThenInclude(s =&gt; s.Profile)</c>. 0 outside an include chain.
    /// </summary>
    public int Matched { get; }

    /// <summary>
    /// The segments in front of the matched ones, which the chain before the ThenIncludes must end at exactly:
    /// ["Settings"] for "Settings.Profile" with "Profile" matched.
    /// </summary>
    public ImmutableArray<string> Remaining => Segments.RemoveRange(Segments.Length - Matched, Matched);

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
    /// <returns>Longer path, with nothing matched.</returns>
    public NavigationPath Prepend(params string[] segments)
        => new(Segments.InsertRange(0, segments));

    /// <summary>
    /// True if the remaining segments end with the chain: ["Settings", "Profile"] ends with ["Profile"] and with
    /// ["Settings", "Profile"].
    /// </summary>
    /// <param name="chain">Property names of an Include or ThenInclude, or the first of them.</param>
    /// <returns>True if the chain matches the end.</returns>
    public bool RemainingEndsWith(IReadOnlyList<string> chain)
    {
        var remaining = Remaining;
        if (chain.Count > remaining.Length)
        {
            return false;
        }

        var start = remaining.Length - chain.Count;
        for (var i = 0; i < chain.Count; i++)
        {
            if (!string.Equals(remaining[start + i], chain[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the same path with more segments at its end matched.
    /// </summary>
    /// <param name="count">Number of segments a ThenInclude matched.</param>
    /// <returns>Path with the cursor moved.</returns>
    public NavigationPath MatchTail(int count) => new(Segments, Matched + count);

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
    /// True if both paths have the same segments and the same cursor. The walker's memory of read blocks compares
    /// paths this way, so a search inside an include chain and the plain one do not share it.
    /// </summary>
    /// <param name="other">Other path.</param>
    /// <returns>True if equal.</returns>
    public bool Equals(NavigationPath? other)
        => other is not null &&
           Matched == other.Matched &&
           Segments.SequenceEqual(other.Segments, StringComparer.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as NavigationPath);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Combines the segment hashes in order, so "A.B" and "B.A" differ. System.HashCode is not available on
        // .NET Standard 2.0; 17 and 31 are the usual odd primes for this.
        var hash = 17 + Matched;

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
