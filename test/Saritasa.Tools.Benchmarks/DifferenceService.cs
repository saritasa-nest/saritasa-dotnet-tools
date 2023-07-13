using System.Collections.Generic;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Benchmarks;

/// <summary>
/// Experimental diff implementation.
/// </summary>
public static class DifferenceService
{
    /// <summary>
    /// Get difference result by entity hash.
    /// </summary>
    /// <typeparam name="T">Identity comparable entity type.</typeparam>
    /// <param name="source">Source collection.</param>
    /// <param name="target">Target collection.</param>
    /// <param name="equalityComparer">Equality comparer.</param>
    public static DiffResult<T> GetDiffResult<T>(
        ICollection<T> source,
        ICollection<T> target,
        IEqualityComparer<T> equalityComparer)
    {
        var sourceDiffHashSet = new HashSet<T>(source);
        var targetDiffHashSet = new HashSet<T>(target);

        sourceDiffHashSet.ExceptWith(target);
        targetDiffHashSet.ExceptWith(source);

        var removed = new List<T>(sourceDiffHashSet.Count);
        var updated = new List<DiffResultUpdatedItems<T>>(sourceDiffHashSet.Count);
        var added = new HashSet<T>(targetDiffHashSet, equalityComparer);

        foreach (var sourceItem in sourceDiffHashSet)
        {
            if (added.TryGetValue(sourceItem, out var targetItem))
            {
                added.Remove(targetItem);
                updated.Add(new DiffResultUpdatedItems<T>(sourceItem, targetItem));
            }
            else
            {
                removed.Add(sourceItem);
            }
        }

        return new DiffResult<T>(added, removed, updated);
    }
}
