// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Paged enumerable.
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

        private IEnumerable<T> source;
        private int totalPages;
        private int currentPage;
        private int pageSize;

        /// <summary>
        /// Total pages.
        /// </summary>
        public int TotalPages
        {
            get { return totalPages; }
        }

        /// <summary>
        /// Current page. Starts from 1.
        /// </summary>
        public int CurrentPage
        {
            get { return currentPage; }
        }

        /// <summary>
        /// Page size. Max number of items on page.
        /// </summary>
        public int PageSize
        {
            get { return pageSize; }
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
        /// <param name="source">Enumerable.</param>
        /// <param name="page">Current page. Default is 1.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="totalPages">Total pages. If below zero it will be calculated.</param>
        public PagedEnumerable(
            IEnumerable<T> source,
            int page = DefaultCurrentPage,
            int pageSize = DefaultPageSize,
            int totalPages = -1)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (page <= 0)
            {
                throw new ArgumentException(nameof(page));
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException(nameof(pageSize));
            }

            this.currentPage = page;
            this.pageSize = pageSize;
            this.source = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            this.totalPages = totalPages > 0 ? totalPages : GetTotalPages(source, PageSize);
        }

        /// <summary>
        /// Creates the instance without any queries. It only fills internal properies.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <param name="page">Page to select. Default is first.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="totalPages">Total pages. If below zero it will be calculated.</param>
        public static PagedEnumerable<T> Create(
            IEnumerable<T> source,
            int page = DefaultCurrentPage,
            int pageSize = DefaultPageSize,
            int totalPages = -1)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (page <= 0)
            {
                throw new ArgumentException(nameof(page));
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException(nameof(pageSize));
            }

            return new PagedEnumerable<T>()
            {
                source = source,
                currentPage = page,
                pageSize = pageSize,
                totalPages = totalPages,
            };
        }

        private static int GetTotalPages(IEnumerable<T> source, int pageSize)
        {
            return (source.Count() + pageSize - 1) / pageSize;
        }

        /// <summary>
        /// Returns enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
