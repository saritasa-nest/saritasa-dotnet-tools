// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Saritasa.Tools.Common.Utils;

/// <summary>
/// The result of collections comparision.
/// </summary>
/// <typeparam name="T">Collections source type.</typeparam>
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
[Serializable]
#endif
[DebuggerDisplay("Added: {Added.Count}, removed: {Removed.Count}, updated: {Updated.Count}")]
public class DiffResult<T>
{
    /// <summary>
    /// Are there ane differences in the diff.
    /// </summary>
    public bool HasDifferences => Added.Count > 0 || Removed.Count > 0 || Updated.Count > 0;

    /// <summary>
    /// New items.
    /// </summary>
    public ICollection<T> Added { get; protected internal set; }

    /// <summary>
    /// Removed items.
    /// </summary>
    public ICollection<T> Removed { get; protected internal set; }

    /// <summary>
    /// Updated items. This is the collection of tuples where first item is
    /// from source collection and second one is from target.
    /// </summary>
    public ICollection<DiffResultUpdatedItems<T>> Updated { get; protected internal set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="added">Added items.</param>
    /// <param name="removed">Removed items.</param>
    /// <param name="updated">Updated items.</param>
    public DiffResult(ICollection<T> added, ICollection<T> removed, ICollection<DiffResultUpdatedItems<T>> updated)
    {
        Added = added ?? new List<T>();
        Removed = removed ?? new List<T>();
        Updated = updated ?? new List<DiffResultUpdatedItems<T>>();
    }

    /// <summary>
    /// Deconstruction method used for tuples.
    /// </summary>
    /// <param name="added">Added items.</param>
    /// <param name="removed">Removed items.</param>
    /// <param name="updated">Updated items.</param>
    public void Deconstruct(out ICollection<T> added, out ICollection<T> removed,
        out ICollection<DiffResultUpdatedItems<T>> updated)
    {
        added = Added;
        removed = Removed;
        updated = Updated;
    }
}
