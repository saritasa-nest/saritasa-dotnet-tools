// Copyright(c) 2015-2017, Saritasa.All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Collection utils.
    /// </summary>
    public static class CollectionUtils
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
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
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
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer,
            SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending ? source.OrderBy(keySelector, comparer) : source.OrderByDescending(keySelector, comparer);
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
            IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }

        /// <summary>
        /// Breaks a list of items into chunks of a specific size. Be aware that this method generates one additional
        /// query to get total number of collection elements.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Enumeration of queryable collections.</returns>
        public static IEnumerable<IQueryable<T>> ChunkSelectRange<T>(
            IQueryable<T> source,
            int chunkSize = DefaultChunkSize)
        {
            long totalNumberOfElements = source.LongCount();
            int currentPosition = 0;
            var originalSource = source;
            while (totalNumberOfElements > currentPosition)
            {
                yield return originalSource.Take(chunkSize);
                originalSource = originalSource.Skip(chunkSize);
                currentPosition += chunkSize;
            }
        }
#endif

        /// <summary>
        /// Breaks a list of items into chunks of a specific size and yeilds T items.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Items of type T.</returns>
        public static IEnumerable<T> ChunkSelect<T>(
            IQueryable<T> source,
            int chunkSize = DefaultChunkSize)
        {
            int currentPosition = 0;
            bool hasRecords;
            do
            {
                var chunkedSource = source.Skip(currentPosition).Take(chunkSize);

                hasRecords = false;
                // Actual query goes here.
                foreach (var item in chunkedSource)
                {
                    hasRecords = true;
                    yield return item;
                }
                currentPosition += chunkSize;
            }
            while (hasRecords);
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
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            IEnumerable<TSource> DistinctByImpl()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                    {
                        yield return element;
                    }
                }
            }

            return DistinctByImpl();
        }
    }
}
