// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination;

/// <summary>
/// Metadata data transfer object. Combines metadata and enumerable items.
/// </summary>
/// <typeparam name="TItem">Metadata items type.</typeparam>
/// <typeparam name="TMetadata">Metadata type.</typeparam>
public interface IMetadataDto<out TItem, out TMetadata>
{
    /// <summary>
    /// Metadata.
    /// </summary>
    TMetadata Metadata { get; }

    /// <summary>
    /// Enumerable items.
    /// </summary>
    IEnumerable<TItem> Items { get; }
}
