// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Paged enumerable. Also it forces evaluation with Take and Skip methods.
    /// </summary>
    /// <typeparam name="T">Source type.</typeparam>
    public class PagedEnumerable<T> : OffsetLimitEnumerable<T>,
        IMetadataEnumerable<PagedEnumerableMetadata, T>
    {
        /// <summary>
        /// Current page. Starts from 1.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Total pages.
        /// </summary>
        public int TotalPages { get; protected internal set; }

        /// <summary>
        /// Page size. Max number of items on page.
        /// </summary>
        public int PageSize { get; protected internal set; }

        /// <summary>
        /// Is pagination now on first page.
        /// </summary>
        public bool IsFirstPage => Page == 1;

        /// <summary>
        /// Is pagination now on last page.
        /// </summary>
        public bool IsLastPage => Page == TotalPages;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        public T this[int index] => ((IList<T>)Source)[index];

        /// <summary>
        /// Internal .ctor
        /// </summary>
        internal PagedEnumerable()
        {
        }

        /// <summary>
        /// Creates paged enumerable from source and query source list by page and pageSize. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Enumerable.</param>
        /// <param name="page">Current page. Default is 1.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="totalCount">Total count of items. If below zero it will be calculated automatically.</param>
        internal PagedEnumerable(
            IEnumerable<T> baseSource,
            int page = PagedEnumerable.DefaultCurrentPage,
            int pageSize = PagedEnumerable.DefaultPageSize,
            int totalCount = -1) : base(baseSource, (page - 1) * pageSize, pageSize, totalCount)
        {
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalPages = GetTotalPages(TotalCount, PageSize);
        }

        /// <summary>
        /// Creates paged enumerable from source and query source list by page and pageSize. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Queryable enumerable.</param>
        /// <param name="page">Current page. Default is 1.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="totalCount">Total count of items. If below zero it will be calculated automatically.</param>
        internal PagedEnumerable(
            IQueryable<T> baseSource,
            int page = PagedEnumerable.DefaultCurrentPage,
            int pageSize = PagedEnumerable.DefaultPageSize,
            int totalCount = -1) : base(baseSource, (page - 1) * pageSize, pageSize, totalCount)
        {
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalPages = GetTotalPages(TotalCount, PageSize);
        }

#if NET452 || NET461 || NETSTANDARD1_2 || NETSTANDARD1_6 || NETSTANDARD2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static int GetTotalPages(int totalItemsCount, int pageSize)
        {
            if (pageSize >= int.MaxValue - totalItemsCount)
            {
                return 1;
            }
            return (totalItemsCount + pageSize - 1) / pageSize;
        }

        #region IMetadataEnumerable<PagedEnumerableMetadata, T>

        /// <summary>
        /// Get paged metadata object.
        /// </summary>
        /// <returns>Paged metadata.</returns>
        public new PagedEnumerableMetadata GetMetadata()
        {
            return new PagedEnumerableMetadata
            {
                TotalPages = TotalPages,
                PageSize = PageSize,
                Page = Page,
                Offset = PagedEnumerable.GetOffset(Page, PageSize),
                Limit = PageSize,
                TotalCount = TotalCount
            };
        }

        /// <inheritdoc />
        public new MetadataDto<T> ToMetadataObject()
        {
            return new MetadataDto<T>(this, this.GetMetadata());
        }

        /// <inheritdoc />
        public new IMetadataEnumerable<PagedEnumerableMetadata, TNew> CastMetadataEnumerable<TNew>(Func<T, TNew> converter)
        {
            return new PagedEnumerable<TNew>
            {
                Source = this.Select(converter),
                TotalCount = this.TotalCount,
                Limit = this.Limit,
                Offset = this.Offset,
                TotalPages = this.TotalPages,
                Page = this.Page,
                PageSize = this.PageSize
            };
        }

        #endregion
    }
}
