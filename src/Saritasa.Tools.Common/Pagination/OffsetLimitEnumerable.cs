// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Class contains static methods for <see cref="OffsetLimitEnumerable{T}" /> and is itended to
    /// simplify instaniation and better API.
    /// </summary>
    public static class OffsetLimitEnumerable
    {
        /// <summary>
        /// The default limit size.
        /// </summary>
        public const int DefaultLimitSize = 100;

        /// <summary>
        /// The default offset.
        /// </summary>
        public const int DefaultOffset = 0;

        /// <summary>
        /// Creates new enumerable with limit and offset properties. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Based enumerable source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        /// <param name="totalCount">Total number of items. If below zero will be evaluated automatically.</param>
        public static OffsetLimitEnumerable<T> Create<T>(
            IEnumerable<T> baseSource,
            int offset = DefaultOffset,
            int limit = DefaultLimitSize,
            int totalCount = -1)
        {
            return new OffsetLimitEnumerable<T>(baseSource, offset, limit, totalCount);
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Based queryable enumerable source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        /// <param name="totalCount">Total number of items. If below zero will be evaluated automatically.</param>
        public static OffsetLimitEnumerable<T> Create<T>(
            IQueryable<T> baseSource,
            int offset = DefaultOffset,
            int limit = DefaultLimitSize,
            int totalCount = -1)
        {
            return new OffsetLimitEnumerable<T>(baseSource, offset, limit, totalCount);
        }

        /// <summary>
        /// Creates instance of enumerable with all items within it.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <returns>Paged enumerable.</returns>
        public static OffsetLimitEnumerable<T> CreateAndReturnAll<T>(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.ToArray();
            return new OffsetLimitEnumerable<T>
            {
                Source = list,
                Offset = 1,
                Limit = list.Length,
                TotalCount = list.Length
            };
        }
    }
}
