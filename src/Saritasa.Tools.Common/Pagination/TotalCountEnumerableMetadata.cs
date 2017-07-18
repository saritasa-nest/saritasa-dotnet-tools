// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Enumerable with total count metadata.
    /// </summary>
#if NET40 || NET452 || NET461
    [Serializable]
#endif
    public class TotalCountEnumerableMetadata
    {
        /// <summary>
        /// Total count of items.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
