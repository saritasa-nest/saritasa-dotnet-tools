// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination;

/// <summary>
/// Paged list. Also, it forces evaluation with Take and Skip methods.
/// </summary>
/// <typeparam name="T">Source type.</typeparam>
/// <seealso cref="IEnumerable{T}" />
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
[Serializable]
#endif
public class PagedList<T> : OffsetLimitList<T>
{
    /// <summary>
    /// First-page index.
    /// </summary>
    public const int FirstPage = 1;

    /// <summary>
    /// Current page. It starts at 1.
    /// </summary>
    public int Page { get; protected internal set; }

    /// <summary>
    /// Page size. Max number of items on page.
    /// </summary>
    public int PageSize { get; protected internal set; }

    /// <summary>
    /// Empty offset limit list.
    /// </summary>
    public static new PagedList<T> Empty { get; } =
        new PagedList<T>(new List<T>(), page: 1, pageSize: 1, totalCount: 0);

    /// <summary>
    /// Total pages.
    /// </summary>
    public int TotalPages
    {
        get
        {
            if (PageSize >= int.MaxValue - TotalCount)
            {
                return FirstPage;
            }
            return (TotalCount + PageSize - 1) / PageSize;
        }
    }

    /// <summary>
    /// Is pagination now on the first page.
    /// </summary>
    public bool IsFirstPage => Page == FirstPage;

    /// <summary>
    /// Is pagination now on the last page.
    /// </summary>
    public bool IsLastPage => Page == TotalPages;

    /// <summary>
    /// Parameterless constructor.
    /// </summary>
    protected PagedList()
    {
    }

#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Constructor for deserialization.
    /// </summary>
    /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
    /// <param name="context">Describes the source and destination of a given serialized stream,
    /// and provides an additional caller-defined context.</param>
    protected PagedList(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
    }
#endif

    /// <summary>
    /// Creates paged enumerable from the collection. The page starts at 1. Page size must be must be
    /// greater than zero.
    /// </summary>
    /// <param name="items">Collection.</param>
    /// <param name="page">The current page.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total number of items in collection.</param>
    public PagedList(
        ICollection<T> items,
        int page,
        int pageSize,
        int totalCount) : base(items, (page - 1) * pageSize, pageSize, totalCount)
    {
        if (page < FirstPage)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }
        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        this.Page = page;
        this.PageSize = pageSize;
    }

    #region IMetadataEnumerable<PagedEnumerableMetadata, T>

    /// <summary>
    /// Get a paged metadata object.
    /// </summary>
    /// <returns>Paged metadata.</returns>
    private PagedListMetadata GetMetadata()
    {
        return new PagedListMetadata
        {
            TotalPages = TotalPages,
            PageSize = PageSize,
            Page = Page,
            Offset = Offset,
            Limit = PageSize,
            TotalCount = TotalCount
        };
    }

    /// <summary>
    /// Create items object with metadata.
    /// </summary>
    /// <returns>List metadata.</returns>
    public new PagedListMetadataDto<T> ToMetadataObject()
        => new PagedListMetadataDto<T>(Items, this.GetMetadata());

    /// <summary>
    /// Convert items into another type.
    /// </summary>
    /// <param name="converter">Converter function.</param>
    /// <typeparam name="TNew">New type.</typeparam>
    /// <returns>New list.</returns>
    public new PagedList<TNew> Convert<TNew>(Func<T, TNew> converter)
        => new PagedList<TNew>
        {
            Items = Items.Select(converter).ToList(),
            Page = Page,
            PageSize = PageSize,
            Limit = Limit,
            Offset = Offset,
            TotalCount = TotalCount
        };

    #endregion
}
