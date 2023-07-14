// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination;

/// <summary>
/// Metadata data transfer object for paged list. Combines metadata and enumerable items.
/// </summary>
/// <typeparam name="TItem">Metadata items type.</typeparam>
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
[Serializable]
#endif
public class PagedListMetadataDto<TItem> : MetadataDto<TItem, PagedListMetadata>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="items">Metadata items.</param>
    /// <param name="metadata">Metadata object.</param>
    public PagedListMetadataDto(IEnumerable<TItem> items, PagedListMetadata metadata) : base(items, metadata)
    {
    }

#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Constructor for deserialization.
    /// </summary>
    /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
    /// <param name="context">Describes the source and destination of a given serialized stream,
    /// and provides an additional caller-defined context.</param>
    protected PagedListMetadataDto(
        System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
        base(info, context)
    {
    }
#endif
}
