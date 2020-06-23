// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// The class contains static methods for <see cref="TotalCountList{T}" /> and is intended to
    /// simplify instantiation and better API.
    /// </summary>
    public static class TotalCountListFactory
    {
        /// <summary>
        /// Creates a list with the total count from enumerable.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Enumerable.</param>
        /// <returns>Total count list.</returns>
        public static TotalCountList<T> FromSource<T>(
            IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.ToList();
            return new TotalCountList<T>(list, list.Count);
        }

        /// <summary>
        /// Creates a list with the total count from collection or list.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Collection enumerable.</param>
        /// <returns>Total count list.</returns>
        public static TotalCountList<T> FromSource<T>(
            ICollection<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new TotalCountList<T>(source, source.Count);
        }

        /// <summary>
        /// Creates a list with the total count from queryable source.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Queryable enumerable.</param>
        /// <returns>Total count list.</returns>
        public static TotalCountList<T> FromSource<T>(
            IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.ToList();
            return new TotalCountList<T>(list, list.Count);
        }

        /// <summary>
        /// Returns an empty list with the total count.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <returns>Empty list with total count.</returns>
        public static TotalCountList<T> Empty<T>() => TotalCountList<T>.Empty;

        /// <summary>
        /// Creates a list with the total count property. Shorthand to simplify type infer.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">Collection items.</param>
        /// <param name="totalCount">The total number of items of base collection.</param>
        /// <returns>List with total count.</returns>
        public static TotalCountList<T> Create<T>(ICollection<T> items, int totalCount)
            => new TotalCountList<T>(items, totalCount);
    }
}
