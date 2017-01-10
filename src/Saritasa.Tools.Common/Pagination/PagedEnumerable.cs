// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Pagination
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    /// <summary>
    /// Paged enumerable. May additionaly query source to get total number of items in collection.
    /// </summary>
    /// <typeparam name="T">Source type.</typeparam>
    public class PagedEnumerable<T> : IEnumerable<T>
    {
        /// <summary>
        /// The default current page.
        /// </summary>
        public const int DefaultCurrentPage = 1;

        /// <summary>
        /// The default size of the page.
        /// </summary>
        public const int DefaultPageSize = 100;

        IList<T> source;
        int totalPages;
        int pageSize;

        /// <summary>
        /// Total pages.
        /// </summary>
        public int TotalPages => totalPages;

        /// <summary>
        /// Current page. Starts from 1.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Page size. Max number of items on page.
        /// </summary>
        public int PageSize => pageSize;

        /// <summary>
        /// Is pagination now on first page.
        /// </summary>
        public bool IsFirstPage => CurrentPage == 1;

        /// <summary>
        /// Is pagination now on last page.
        /// </summary>
        public bool IsLastPage => CurrentPage == TotalPages;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        public T this[int index] => source[index];

        /// <summary>
        /// Get paged metadata.
        /// </summary>
        /// <returns>Paged metadata.</returns>
        public PagedMetadata GetMetadata()
        {
            return new PagedMetadata
            {
                TotalPages = TotalPages,
                PageSize = PageSize,
                CurrentPage = CurrentPage,
            };
        }

        /// <summary>
        /// Internal .ctor
        /// </summary>
        internal PagedEnumerable()
        {
        }

        /// <summary>
        /// Creates paged enumerable from source and query source list by page and pageSize.
        /// </summary>
        /// <param name="baseSource">Enumerable.</param>
        /// <param name="page">Current page. Default is 1.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="totalPages">Total pages. If below zero it will be calculated.</param>
        public PagedEnumerable(
            [NotNull] IEnumerable<T> baseSource,
            int page = DefaultCurrentPage,
            int pageSize = DefaultPageSize,
            int totalPages = -1)
        {
            if (baseSource == null)
            {
                throw new ArgumentNullException(nameof(baseSource));
            }
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            this.CurrentPage = page;
            this.pageSize = pageSize;
            this.totalPages = totalPages > 0 ? totalPages : GetTotalPages(baseSource, PageSize);
            this.source = baseSource.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// Creates instance with position on first page. PageSize will be set to total
        /// number of records in source.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <returns>Paged enumerable.</returns>
        public static PagedEnumerable<T> CreateAndReturnAll([NotNull] IEnumerable<T> source)
        {
            var list = source.ToList();
            return new PagedEnumerable<T>()
            {
                source = list,
                CurrentPage = 1,
                pageSize = list.Count,
                totalPages = list.Count,
            };
        }

        static int GetTotalPages([NotNull] IEnumerable<T> source, int pageSize)
        {
            return (source.Count() + pageSize - 1) / pageSize;
        }

        /// <summary>
        /// Returns enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
