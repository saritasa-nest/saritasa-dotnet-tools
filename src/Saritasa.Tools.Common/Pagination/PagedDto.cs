// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Pagination
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Paged data transfer object. Combines metadata and page items.
    /// </summary>
    /// <typeparam name="T">Page items type.</typeparam>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_2 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class PagedDto<T>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public PagedDto()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="items">Page items.</param>
        /// <param name="metadata">Page metadata.</param>
        public PagedDto(IEnumerable<T> items, PagedMetadata metadata)
        {
            Items = items;
            Metadata = metadata;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="page">Paged enumerable.</param>
        public PagedDto(PagedEnumerable<T> page)
        {
            Items = page;
            Metadata = page.GetMetadata();
        }

        /// <summary>
        /// Page metadata.
        /// </summary>
        public PagedMetadata Metadata { get; set; }

        /// <summary>
        /// Page items.
        /// </summary>
        public IEnumerable<T> Items { get; set; }
    }
}
