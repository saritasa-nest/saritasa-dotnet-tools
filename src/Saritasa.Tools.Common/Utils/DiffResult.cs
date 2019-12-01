// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// The result of collections comparision.
    /// </summary>
    /// <typeparam name="T">Collections source type.</typeparam>
#if NET40 || NETSTANDARD2_0
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
        public ICollection<T> Added { get; internal set; }

        /// <summary>
        /// Removed items.
        /// </summary>
        public ICollection<T> Removed { get; internal set; }

        /// <summary>
        /// Updated items. This is the collection of tuples where first item is
        /// from source collection and second one is from target.
        /// </summary>
        public ICollection<(T Source, T Target)> Updated { get; internal set; }
    }
}
