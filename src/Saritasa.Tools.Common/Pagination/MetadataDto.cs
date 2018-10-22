// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Metadata data transfer object. Combines metadata and enumerable items.
    /// </summary>
    /// <typeparam name="TItem">Metadata items type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
#if NET40 || NET452 || NET461 || NETSTANDARD2_0
    [Serializable]
#endif
    public class MetadataDto<TItem, TMetadata>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">Metadata items.</param>
        /// <param name="metadata">Metadata object.</param>
        public MetadataDto(IEnumerable<TItem> items, TMetadata metadata)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            Items = items;
            Metadata = metadata;
        }

        /// <summary>
        /// Metadata.
        /// </summary>
        public TMetadata Metadata { get; protected set; }

        /// <summary>
        /// Enumerable items.
        /// </summary>
        public IEnumerable<TItem> Items { get; protected set; }
    }
}
