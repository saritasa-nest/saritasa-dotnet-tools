// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Enumerable with total count metadata.
    /// </summary>
#if NET40 || NET452 || NET461 || NETSTANDARD2_0
    [Serializable]
#endif
    public class TotalCountListMetadata
    {
        /// <summary>
        /// Total count of items.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
