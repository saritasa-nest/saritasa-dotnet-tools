// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Class contains static methods for <see cref="TotalCountEnumerable{T}" /> and is itended to
    /// simplify instaniation and better API.
    /// </summary>
    public static class TotalCountEnumerable
    {
        /// <summary>
        /// Creates enumerable with total count.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <param name="totalCount">Total count of items of base collection.</param>
        public static TotalCountEnumerable<T> Create<T>(
            IEnumerable<T> source,
            int totalCount = -1)
        {
            return new TotalCountEnumerable<T>(source, totalCount);
        }

        /// <summary>
        /// Creates enumerable with total count.
        /// </summary>
        /// <param name="source">Queryable enumerable.</param>
        /// <param name="totalCount">Total count of items of base collection. If below zero query
        /// will be evaluated automatically.</param>
        public static TotalCountEnumerable<T> Create<T>(
            IQueryable<T> source,
            int totalCount = -1)
        {
            return new TotalCountEnumerable<T>(source, totalCount);
        }
    }
}
