// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Metadata data transfer object. Combines metadata and enumerable items.
    /// </summary>
    /// <typeparam name="T">Metadata items type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
#if NET40 || NET452 || NET461
    [Serializable]
#endif
    public class MetadataDto<T, TMetadata>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public MetadataDto()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="items">Page items.</param>
        /// <param name="metadata">Page metadata.</param>
        public MetadataDto(IEnumerable<T> items, IMetadataEnumerable<TMetadata> metadata)
        {
            Data = items;
            Metadata = metadata;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="page">Enumerable.</param>
        public MetadataDto(PagedEnumerable<T> page)
        {
            Data = page;
            Metadata = page.GetMetadata();
        }

        /// <summary>
        /// Metadata.
        /// </summary>
        public object Metadata { get; set; }

        /// <summary>
        /// Enumerable items.
        /// </summary>
        public IEnumerable<T> Data { get; set; }
    }
}
