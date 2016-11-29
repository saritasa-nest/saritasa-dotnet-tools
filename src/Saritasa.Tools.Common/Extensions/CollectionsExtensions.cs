// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Sort order enumeration.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Ascending order of sort.
        /// </summary>
        Asc = 0,

        /// <summary>
        /// Descending order of sort.
        /// </summary>
        Desc = 1,
    }

    /// <summary>
    /// Collections extensions.
    /// </summary>
    public static class CollectionsExtensions
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
        /// <returns>An System.Linq.IOrderedEnumerable whose elements are sorted according to a key.</returns>
        public static IOrderedEnumerable<TSource> Order<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Asc ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
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
        /// <returns>An System.Linq.IOrderedEnumerable whose elements are sorted according to a key.</returns>
        public static IOrderedEnumerable<TSource> Order<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer,
            SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Asc ? source.OrderBy(keySelector, comparer) : source.OrderByDescending(keySelector, comparer);
        }

        /// <summary>
        /// Get paged enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="source">Enumerable source to be paginate.</param>
        /// <param name="page">Page number to select from source.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged enumerable.</returns>
        public static PagedEnumerable<T> GetPaged<T>(
            this IEnumerable<T> source,
            int page = PagedEnumerable<T>.DefaultCurrentPage,
            int pageSize = PagedEnumerable<T>.DefaultPageSize)
        {
            return new PagedEnumerable<T>(source, page, pageSize);
        }

        /// <summary>
        /// Breaks a list of items into chunks of a specific size.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Enumeration of iterators.</returns>
        public static IEnumerable<IEnumerable<T>> ChunkSelectRange<T>(this IEnumerable<T> source, int chunkSize = DefaultChunkSize)
        {
            var originalSource = source;
            while (originalSource.Any())
            {
                yield return originalSource.Take(chunkSize);
                originalSource = originalSource.Skip(chunkSize);
            }
        }

#if !NETSTANDARD1_2 && !NETSTANDARD1_6
        /// <summary>
        /// Breaks a list of items into chunks of a specific size. Be aware that this method generates one additional
        /// sql query to get total number of collection elements.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Enumeration of queryables.</returns>
        public static IEnumerable<IQueryable<T>> ChunkSelectRange<T>(this IQueryable<T> source, int chunkSize = DefaultChunkSize)
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
        /// Breaks a list of items into chunks of a specific size and yields T items.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Items of type T.</returns>
        public static IEnumerable<T> ChunkSelect<T>(this IEnumerable<T> source, int chunkSize = DefaultChunkSize)
        {
            var currentPosition = 0;
            IEnumerable<T> subsource;
            bool hasRecords;
            do
            {
                subsource = source.Skip(currentPosition).Take(chunkSize);
                hasRecords = false;
                foreach (var item in subsource)
                {
                    hasRecords = true;
                    yield return item;
                }
                currentPosition += chunkSize;
            }
            while (hasRecords);
        }

#if !NETSTANDARD1_2 && !NETSTANDARD1_6
        /// <summary>
        /// Breaks a list of items into chunks of a specific size and yeilds T items.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Items of type T.</returns>
        public static IEnumerable<T> ChunkSelect<T>(this IQueryable<T> source, int chunkSize = DefaultChunkSize)
        {
            int currentPosition = 0;
            bool hasRecords;
            var originalSource = source;
            do
            {
                originalSource = originalSource.Skip(currentPosition).Take(chunkSize);
                hasRecords = false;
                // actual query is here
                foreach (var item in originalSource)
                {
                    hasRecords = true;
                    yield return item;
                }
                currentPosition += chunkSize;
            }
            while (hasRecords);
        }
#endif

        /// <summary>
        /// ForEach extension for IEnumerable of T. It's equivalence to foreach loop.
        /// </summary>
        /// <param name="target">Target collection.</param>
        /// <param name="action">Action for execute on each item.</param>
        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action)
        {
            foreach (T item in target)
            {
                action(item);
            }
        }
    }
}
