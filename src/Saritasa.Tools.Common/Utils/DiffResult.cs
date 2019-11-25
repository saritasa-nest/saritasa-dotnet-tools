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
        /// New items to add.
        /// </summary>
        public ICollection<T> Added { get; internal set; }

        /// <summary>
        /// Items to remove.
        /// </summary>
        public ICollection<T> Removed { get; internal set; }

        /// <summary>
        /// Items to update. This is the collection of tuples where first item is
        /// from source collection and second one is from target.
        /// </summary>
        public ICollection<(T source, T target)> Updated { get; internal set; }
    }
}
