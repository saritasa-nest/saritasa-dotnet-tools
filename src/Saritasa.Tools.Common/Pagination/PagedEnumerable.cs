// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Class contains static methods for <see cref="PagedEnumerable{T}" /> and is itended to
    /// simplify instaniation and better API.
    /// </summary>
    public static class PagedEnumerable
    {
        /// <summary>
        /// The default current page.
        /// </summary>
        public const int DefaultCurrentPage = 1;

        /// <summary>
        /// The default size of the page.
        /// </summary>
        public const int DefaultPageSize = 100;

        /// <summary>
        /// Creates paged enumerable from source and query source list by page and pageSize. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Enumerable.</param>
        /// <param name="page">Current page. Default is 1.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="totalCount">Total count of items. If below zero it will be calculated automatically.</param>
        public static PagedEnumerable<T> Create<T>(
            IEnumerable<T> baseSource,
            int page = DefaultCurrentPage,
            int pageSize = DefaultPageSize,
            int totalCount = -1)
        {
            return new PagedEnumerable<T>(baseSource, page, pageSize, totalCount);
        }

        /// <summary>
        /// Creates paged enumerable from source and query source list by page and pageSize. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Queryable enumerable.</param>
        /// <param name="page">Current page. Default is 1.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="totalCount">Total count of items. If below zero it will be calculated automatically.</param>
        public static PagedEnumerable<T> Create<T>(
            IQueryable<T> baseSource,
            int page = DefaultCurrentPage,
            int pageSize = DefaultPageSize,
            int totalCount = -1)
        {
            return new PagedEnumerable<T>(baseSource, page, pageSize, totalCount);
        }

        /// <summary>
        /// Creates instance with position on first page. PageSize will be set to total
        /// number of records in source.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <returns>Paged enumerable.</returns>
        public static PagedEnumerable<T> CreateAndReturnAll<T>(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.ToArray();
            return new PagedEnumerable<T>
            {
                Source = list,
                Page = 1,
                PageSize = list.Length,
                TotalPages = 1,
                TotalCount = list.Length,
            };
        }

        /// <summary>
        /// Creates instance without any evaluation. Can be useful when source enumerable is already
        /// transformed and need to be wrapped only.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <param name="page">Current page.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalCount">Total count of items that were in base source enumerable.</param>
        /// <returns>Class instance.</returns>
        public static PagedEnumerable<TTarget> Wrap<TTarget>(
            IEnumerable<TTarget> source,
            int page,
            int pageSize,
            int totalCount)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }
            if (totalCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCount));
            }

            return new PagedEnumerable<TTarget>
            {
                Page = page,
                PageSize = pageSize,
                Offset = GetOffset(page, pageSize),
                Limit = pageSize,
                Source = source,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Creates instance without any evaluation with new enumerable source.
        /// Can be useful when source enumerable is already transformed and need to be wrapped only.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <param name="pagedEnumerable">Existing paged enumerable.</param>
        /// <returns>Class instance.</returns>
        public static PagedEnumerable<TTarget> Wrap<TSource, TTarget>(
            IEnumerable<TTarget> source,
            PagedEnumerable<TSource> pagedEnumerable)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (pagedEnumerable == null)
            {
                throw new ArgumentNullException(nameof(pagedEnumerable));
            }

            return new PagedEnumerable<TTarget>
            {
                Page = pagedEnumerable.Page,
                PageSize = pagedEnumerable.PageSize,
                Offset = GetOffset(pagedEnumerable.Page, pagedEnumerable.PageSize),
                Limit = pagedEnumerable.Limit,
                Source = source,
                TotalCount = pagedEnumerable.TotalCount
            };
        }

        internal static int GetOffset(int page, int pageSize) => (page - 1) * pageSize;
    }
}
