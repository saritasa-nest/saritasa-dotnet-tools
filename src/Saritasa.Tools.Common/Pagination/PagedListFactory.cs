// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Class contains static methods for <see cref="PagedList{T}" /> and is intended to
    /// simplify instantiation and better API.
    /// </summary>
    public static class PagedListFactory
    {
        /// <summary>
        /// Creates paged enumerable from enumerable source.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Enumerable.</param>
        /// <param name="page">Current page.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged list.</returns>
        public static PagedList<T> FromSource<T>(
            IEnumerable<T> source,
            int page,
            int pageSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var offset = GetOffset(page, pageSize);
            return new PagedList<T>(source.Skip(offset).Take(pageSize).ToList(), page, pageSize,
                source.Count());
        }

        /// <summary>
        /// Creates paged enumerable from collection source.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Collection.</param>
        /// <param name="page">Current page.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged list.</returns>
        public static PagedList<T> FromSource<T>(
            ICollection<T> source,
            int page,
            int pageSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var offset = GetOffset(page, pageSize);
            return new PagedList<T>(source.Skip(offset).Take(pageSize).ToList(), page, pageSize,
                source.Count);
        }

        /// <summary>
        /// Creates paged list from queryable source and query source by page and page size.
        /// The calling will evaluate query automatically.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Queryable enumerable.</param>
        /// <param name="page">Current page.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged list.</returns>
        public static PagedList<T> FromSource<T>(
            IQueryable<T> source,
            int page,
            int pageSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var offset = GetOffset(page, pageSize);
            return new PagedList<T>(source.Skip(offset).Take(pageSize).ToList(), page, pageSize,
                source.Count());
        }

        /// <summary>
        /// Returns collection of items as paged list as one page.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">Items.</param>
        /// <returns>Paged list.</returns>
        public static PagedList<T> AsOnePage<T>(
            ICollection<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return new PagedList<T>(items, PagedList<object>.FirstPage, items.Count, items.Count);
        }

        /// <summary>
        /// Returns empty paged collection.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <returns>Empty paged list.</returns>
        public static PagedList<T> Empty<T>()
            => new PagedList<T>(Enumerable.Empty<T>() as ICollection<T>, page: PagedList<object>.FirstPage, pageSize: 0, totalCount: 0);

        /// <summary>
        /// Creates paged enumerable from collection. Shorthand to simplify type infer.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">Items.</param>
        /// <param name="page">Current page.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalCount">Total number of items in collection.</param>
        /// <returns>Paged list.</returns>
        public static PagedList<T> Create<T>(ICollection<T> items, int page, int pageSize, int totalCount)
            => new PagedList<T>(items, page, pageSize, totalCount);

        internal static int GetOffset(int page, int pageSize)
        {
            if (page < PagedList<object>.FirstPage)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            return (page - 1) * pageSize;
        }
    }
}
