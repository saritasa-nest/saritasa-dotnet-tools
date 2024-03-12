// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Extensions;

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
        ListSortDirection sortOrder)
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
        ListSortDirection sortOrder)
    {
        return CollectionUtils.Order(source, keySelector, comparer, sortOrder);
    }

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
        ListSortDirection sortOrder)
    {
        return CollectionUtils.Order(source, keySelector, sortOrder);
    }

    /// <summary>
    /// Breaks a list of items into chunks of a specific size. Be aware that this method generates one additional
    /// SQL query to get the total number of collection elements.
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

    /// <summary>
    /// Returns distinct elements from a sequence by using a specified <see cref="IEqualityComparer{T}" />
    /// to compare values and specified key selector to get a distinct key. This method is implemented by
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
        IEqualityComparer<TKey>? comparer = null)
    {
        return CollectionUtils.DistinctBy(source, keySelector, comparer);
    }

    /// <summary>
    /// The extension allows adding whole enumerable collection into the target collection.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <param name="target">The target collection to insert.</param>
    /// <param name="collection">The source collection items to be inserted.</param>
    public static void Add<TSource>(this ICollection<TSource> target, IEnumerable<TSource> collection)
    {
        // Much more optimized version to insert for List.
        if (target is List<TSource> listTarget)
        {
            listTarget.AddRange(collection);
            return;
        }

        foreach (var item in collection)
        {
            target.Add(item);
        }
    }

    /// <summary>
    /// Implements the left join operation between two sequences.
    /// </summary>
    /// <param name="outer">The first sequence to join.</param>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from an element from the first sequence
    /// and a collection of matching elements from the second sequence.</param>
    /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <returns>Enumerable the contains the elements of type <see ref="TResult" />.</returns>
    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
        return outer
            .GroupJoin(inner,
                outerKeySelector,
                innerKeySelector,
                (outerObject, inners) => new
                {
                    outerObject,
                    inners = inners.DefaultIfEmpty()
                })
            .SelectMany(a => a.inners.Select(innerObj => resultSelector(a.outerObject, innerObj!)));
    }

    /// <summary>
    /// Implements the left join operation between two sequences.
    /// </summary>
    /// <param name="outer">The first sequence to join.</param>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from an element from the first sequence
    /// and a collection of matching elements from the second sequence.</param>
    /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <returns>Enumerable the contains the elements of type <see ref="TResult" />.</returns>
    public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IQueryable<TOuter> outer,
        IQueryable<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKeySelector,
        Expression<Func<TInner, TKey>> innerKeySelector,
        Expression<Func<Tuple<TOuter, TInner>, TResult>> resultSelector)
    {
        return outer
            .GroupJoin(inner,
                outerKeySelector,
                innerKeySelector,
                (outerObject, inners) => new
                {
                    outerObject,
                    inners = inners.DefaultIfEmpty()
                })
            .SelectMany(row => row.inners, (row, inner) => new Tuple<TOuter, TInner>(row.outerObject, inner!))
            .Select(resultSelector);
    }
}
