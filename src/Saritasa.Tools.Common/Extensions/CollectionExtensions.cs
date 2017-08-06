// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools.Common.Utils;
using System.Linq.Expressions;

namespace Saritasa.Tools.Common.Extensions
{
    /// <summary>
    /// Collection extensions.
    /// </summary>
    public static class CollectionExtensions
    {
        private const int DefaultChunkSize = 1000;

        /// <summary>
        /// Sorts the elements of a sequence in ascending or descending order.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="sortOrder">Sort order.</param>
        /// <returns>An <see cref="System.Linq.IOrderedEnumerable{T}" /> whose elements are sorted according to a key.</returns>
        public static IOrderedEnumerable<TSource> Order<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            SortOrder sortOrder)
        {
            return CollectionUtils.Order(source, keySelector, sortOrder);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending or descending order by using a specified comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">An System.Collections.Generic.IComparer to compare keys.</param>
        /// <param name="sortOrder">Sort order.</param>
        /// <returns>An <see cref="System.Linq.IOrderedEnumerable{T}" /> whose elements are sorted according to a key.</returns>
        public static IOrderedEnumerable<TSource> Order<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer,
            SortOrder sortOrder)
        {
            return CollectionUtils.Order(source, keySelector, comparer, sortOrder);
        }

#if NET40 || NET452 || NET461
        /// <summary>
        /// Sorts the elements of a sequence in ascending or descending order.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="sortOrder">Sort order.</param>
        /// <returns>An <see cref="System.Linq.IOrderedQueryable{T}" /> whose elements are sorted according to a key.</returns>
        public static IOrderedQueryable<TSource> Order<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            SortOrder sortOrder)
        {
            return CollectionUtils.Order(source, keySelector, sortOrder);
        }

        /// <summary>
        /// Breaks a list of items into chunks of a specific size. Be aware that this method generates one additional
        /// sql query to get total number of collection elements.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Enumeration of queryable collections.</returns>
        public static IEnumerable<IQueryable<T>> ChunkSelectRange<T>(
            this IQueryable<T> source,
            int chunkSize = DefaultChunkSize)
        {
            return CollectionUtils.ChunkSelectRange(source, chunkSize);
        }
#endif

        /// <summary>
        /// Breaks a list of items into chunks of a specific size and yields T items.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Items of type T.</returns>
        public static IEnumerable<T> ChunkSelect<T>(
            this IQueryable<T> source,
            int chunkSize = DefaultChunkSize)
        {
            return CollectionUtils.ChunkSelect(source, chunkSize);
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using a specified <see cref="IEqualityComparer{T}" />
        /// to compare values and specified key selector to get distinct key. This method is implemented by
        /// using deferred execution.
        /// </summary>
        /// <typeparam name="TSource">Type of the source sequence.</typeparam>
        /// <typeparam name="TKey">Type of the projected element.</typeparam>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <param name="keySelector">Projection for determining "distinctness".</param>
        /// <param name="comparer">The equality comparer to compare key values. If null default comparer will be used.</param>
        /// <returns>A collection that contains distinct elements from the source sequence.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return CollectionUtils.DistinctBy(source, keySelector, comparer);
        }
    }
}
