// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Extension methods for pagination.
    /// </summary>
    public static class PaginationExtensions
    {
        /// <summary>
        /// Get paged enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="baseSource">Enumerable source to be paginate.</param>
        /// <param name="page">Page number to select from source.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged enumerable.</returns>
        public static PagedEnumerable<T> ToPaged<T>(
            this IEnumerable<T> baseSource,
            int page = PagedEnumerable.DefaultCurrentPage,
            int pageSize = PagedEnumerable.DefaultPageSize)
        {
            return new PagedEnumerable<T>(baseSource, page, pageSize);
        }

        /// <summary>
        /// Get paged query enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="baseSource">Enumerable source to be paginate.</param>
        /// <param name="page">Page number to select from source.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged enumerable.</returns>
        public static PagedEnumerable<T> ToPaged<T>(
            this IQueryable<T> baseSource,
            int page = PagedEnumerable.DefaultCurrentPage,
            int pageSize = PagedEnumerable.DefaultPageSize)
        {
            return new PagedEnumerable<T>(baseSource, page, pageSize);
        }

        /// <summary>
        /// Get paged enumerable result where only one page with all data.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="baseSource">Enumerable source to be paginate.</param>
        /// <returns>Paged enumerable with one page.</returns>
        public static PagedEnumerable<T> ToOnePaged<T>(
            this IEnumerable<T> baseSource)
        {
            return PagedEnumerable.CreateAndReturnAll(baseSource);
        }

        /// <summary>
        /// Get paged query enumerable result where only one page with all data.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="baseSource">Enumerable source to be paginate.</param>
        /// <returns>Paged enumerable with one page.</returns>
        public static PagedEnumerable<T> ToOnePaged<T>(
            this IQueryable<T> baseSource)
        {
            return PagedEnumerable.CreateAndReturnAll(baseSource);
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Based enumerable source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        public static OffsetLimitEnumerable<T> ToOffsetLimit<T>(
            this IEnumerable<T> baseSource,
            int offset = OffsetLimitEnumerable.DefaultOffset,
            int limit = OffsetLimitEnumerable.DefaultLimitSize)
        {
            return new OffsetLimitEnumerable<T>(baseSource, offset, limit);
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Based queryable enumerable source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        public static OffsetLimitEnumerable<T> ToOffsetLimit<T>(
            this IQueryable<T> baseSource,
            int offset = OffsetLimitEnumerable.DefaultOffset,
            int limit = OffsetLimitEnumerable.DefaultLimitSize)
        {
            return new OffsetLimitEnumerable<T>(baseSource, offset, limit);
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties and all evaluted data in it.
        /// </summary>
        /// <param name="baseSource">Based enumerable source.</param>
        public static OffsetLimitEnumerable<T> ToOneOffsetLimit<T>(
            this IEnumerable<T> baseSource)
        {
            return OffsetLimitEnumerable.CreateAndReturnAll(baseSource);
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties and all data in it.
        /// </summary>
        /// <param name="baseSource">Based queryable enumerable source.</param>
        public static OffsetLimitEnumerable<T> ToOneOffsetLimit<T>(
            this IQueryable<T> baseSource)
        {
            return OffsetLimitEnumerable.CreateAndReturnAll(baseSource);
        }
    }
}
