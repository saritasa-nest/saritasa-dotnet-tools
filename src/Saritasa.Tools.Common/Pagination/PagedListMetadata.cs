// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Pagination metadata class.
    /// </summary>
#if NET40 || NET452 || NET461 || NETSTANDARD2_0
    [Serializable]
#endif
    public class PagedListMetadata : OffsetLimitListMetadata
    {
        /// <summary>
        /// Current page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Page size. Max number of items on page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total pages.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
