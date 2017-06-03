// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Extension methods for pagination.
    /// </summary>
    public static class PagedExtensions
    {
        /// <summary>
        /// Get paged enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="source">Enumerable source to be paginate.</param>
        /// <param name="page">Page number to select from source.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged enumerable.</returns>
        public static PagedEnumerable<T> AsPage<T>(
            this IEnumerable<T> source,
            int page = PagedEnumerable<T>.DefaultCurrentPage,
            int pageSize = PagedEnumerable<T>.DefaultPageSize)
        {
            return new PagedEnumerable<T>(source, page, pageSize);
        }

        /// <summary>
        /// Get paged enumerable result where only one page with all data.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="source">Enumerable source to be paginate.</param>
        /// <returns>Paged enumerable with one page.</returns>
        public static PagedEnumerable<T> AsOnePage<T>(
            this IEnumerable<T> source)
        {
            return PagedEnumerable<T>.CreateAndReturnAll(source);
        }
    }
}
