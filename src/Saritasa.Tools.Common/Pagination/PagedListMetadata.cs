// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Pagination metadata class.
    /// </summary>
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
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
