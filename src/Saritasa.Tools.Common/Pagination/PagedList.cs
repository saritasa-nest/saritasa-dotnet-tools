// Copyright (c) 2015-2018, Saritasa. All rights reserved.
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
    public class PagedList<T> : OffsetLimitList<T>,
        IMetadataEnumerable<T, PagedListMetadata>
    {
        /// <summary>
        /// First page index.
        /// </summary>
        public const int FirstPage = 1;

        /// <summary>
        /// Current page. Starts from 1.
        /// </summary>
        public int Page { get; protected internal set; }

        /// <summary>
        /// Page size. Max number of items on page.
        /// </summary>
        public int PageSize { get; protected internal set; }

        /// <summary>
        /// Total pages.
        /// </summary>
        public int TotalPages
        {
            get
            {
                if (PageSize >= int.MaxValue - TotalCount)
                {
                    return FirstPage;
                }
                return (TotalCount + PageSize - 1) / PageSize;
            }
        }

        /// <summary>
        /// Is pagination now on first page.
        /// </summary>
        public bool IsFirstPage => Page == FirstPage;

        /// <summary>
        /// Is pagination now on last page.
        /// </summary>
        public bool IsLastPage => Page == TotalPages;

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        protected PagedList()
        {
        }

        /// <summary>
        /// Creates paged enumerable from source.
        /// </summary>
        /// <param name="items">Collection.</param>
        /// <param name="page">Current page.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalCount">Total number of items in collection.</param>
        public PagedList(
            ICollection<T> items,
            int page,
            int pageSize,
            int totalCount) : base(items, (page - 1) * pageSize, pageSize, totalCount)
        {
            if (page < FirstPage)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            this.Page = page;
            this.PageSize = pageSize;
        }

        #region IMetadataEnumerable<PagedEnumerableMetadata, T>

        /// <summary>
        /// Get paged metadata object.
        /// </summary>
        /// <returns>Paged metadata.</returns>
        private PagedListMetadata GetMetadata()
        {
            return new PagedListMetadata
            {
                TotalPages = TotalPages,
                PageSize = PageSize,
                Page = Page,
                Offset = Offset,
                Limit = PageSize,
                TotalCount = TotalCount
            };
        }

        /// <inheritdoc />
        public new MetadataDto<T, PagedListMetadata> ToMetadataObject()
            => new MetadataDto<T, PagedListMetadata>(Items, this.GetMetadata());

        /// <inheritdoc />
        public new IMetadataEnumerable<TNew, PagedListMetadata> Convert<TNew>(Func<T, TNew> converter)
            => new PagedList<TNew>
            {
                Items = Items.Select(converter).ToList(),
                Page = Page,
                PageSize = PageSize,
                Limit = Limit,
                Offset = Offset,
                TotalCount = TotalCount
            };

        #endregion
    }
}
