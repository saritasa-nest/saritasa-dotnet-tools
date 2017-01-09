// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Pagination
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

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
        public static PagedEnumerable<T> GetPaged<T>(
            [NotNull] this IEnumerable<T> source,
            int page = PagedEnumerable<T>.DefaultCurrentPage,
            int pageSize = PagedEnumerable<T>.DefaultPageSize)
        {
            return new PagedEnumerable<T>(source, page, pageSize);
        }
    }
}
