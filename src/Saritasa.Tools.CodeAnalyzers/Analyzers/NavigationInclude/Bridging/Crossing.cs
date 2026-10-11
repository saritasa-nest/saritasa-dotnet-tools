using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// One crossed <see cref="Bridge"/>: the value the search continues from, and what goes in front of the path there.
/// </summary>
internal sealed class Crossing
{
    /// <summary>
    /// Initializes the crossing.
    /// </summary>
    /// <param name="source">Value the entities came from.</param>
    /// <param name="prefix">Properties to put in front of the path, empty when it stays as it is.</param>
    public Crossing(IOperation source, IReadOnlyList<string> prefix)
    {
        Source = source;
        Prefix = prefix;
    }

    /// <summary>
    /// Value the entities came from: the <c>users</c> of <c>users.Where(...)</c>.
    /// </summary>
    public IOperation Source { get; }

    /// <summary>
    /// Properties to put in front of the path: ["Settings"] for <c>users.Select(u =&gt; u.Settings)</c>, empty for
    /// a bridge that keeps the same entities.
    /// </summary>
    public IReadOnlyList<string> Prefix { get; }
}
