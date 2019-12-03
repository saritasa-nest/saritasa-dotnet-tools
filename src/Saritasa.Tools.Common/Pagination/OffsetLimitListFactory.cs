// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Class contains static methods for <see cref="OffsetLimitList{T}" /> and is intended to
    /// simplify instantiation and better API.
    /// </summary>
    public static class OffsetLimitListFactory
    {
        /// <summary>
        /// Creates new list with limit and offset properties from enumerable source.
        /// The calling will evaluate query automatically.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Enumerable source.</param>
        /// <param name="offset">Number of items to skip.</param>
        /// <param name="limit">Maximum number of items to take.</param>
        /// <returns>List with offset and limit.</returns>
        public static OffsetLimitList<T> FromSource<T>(
            IEnumerable<T> source,
            int offset,
            int limit)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.Skip(offset).Take(limit).ToList();
            return new OffsetLimitList<T>(list, offset, limit, source.Count());
        }

        /// <summary>
        /// Creates new list with limit and offset properties from collection source.
        /// The calling will evaluate query automatically.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Collection source.</param>
        /// <param name="offset">Number of items to skip.</param>
        /// <param name="limit">Maximum number of items to take.</param>
        /// <returns>List with offset and limit.</returns>
        public static OffsetLimitList<T> FromSource<T>(
            ICollection<T> source,
            int offset,
            int limit)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.Skip(offset).Take(limit).ToList();
            return new OffsetLimitList<T>(list, offset, limit, source.Count);
        }

        /// <summary>
        /// Creates new list with limit and offset properties from queryable source.
        /// The calling will evaluate query automatically.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Queryable source.</param>
        /// <param name="offset">Number of items to skip.</param>
        /// <param name="limit">Maximum number of items to take.</param>
        /// <returns>List with offset and limit.</returns>
        public static OffsetLimitList<T> FromSource<T>(
            IQueryable<T> source,
            int offset,
            int limit)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.Skip(offset).Take(limit).ToList();
            return new OffsetLimitList<T>(list, offset, limit, source.Count());
        }

        /// <summary>
        /// Returns empty list with limit and offset properties.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <returns>Empty list with offset and limit.</returns>
        public static OffsetLimitList<T> Empty<T>() => OffsetLimitList<T>.Empty;

        /// <summary>
        /// Creates new list with limit and offset properties. Shorthand to simplify type infer.
        /// </summary>
        /// <param name="items">Collection items.</param>
        /// <param name="offset">Number of items to skip.</param>
        /// <param name="limit">Maximum number of items to take.</param>
        /// <param name="totalCount">Total number of items in collection.</param>
        /// <returns>List with offset and limit.</returns>
        public static OffsetLimitList<T> Create<T>(ICollection<T> items, int offset, int limit, int totalCount)
            => new OffsetLimitList<T>(items, offset, limit, totalCount);
    }
}
