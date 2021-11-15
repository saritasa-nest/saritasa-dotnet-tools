// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
#if NETSTANDARD1_6_OR_GREATER || NET5_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Metadata data transfer object. Combines metadata and enumerable items.
    /// </summary>
    /// <typeparam name="TItem">Metadata items type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    [Serializable]
#endif
    public class MetadataDto<TItem, TMetadata> : IMetadataDto<TItem, TMetadata>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">Metadata items.</param>
        /// <param name="metadata">Metadata object.</param>
        public MetadataDto(IEnumerable<TItem> items, TMetadata metadata)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        protected MetadataDto(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }
#endif

        /// <inheritdoc />
#if NETSTANDARD1_6_OR_GREATER || NET5_0_OR_GREATER
        [Required]
#endif
#if NET5_0_OR_GREATER
        [System.Text.Json.Serialization.JsonInclude]
#endif
        public TMetadata Metadata { get; protected set; }

        /// <inheritdoc />
#if NETSTANDARD1_6_OR_GREATER || NET5_0_OR_GREATER
        [Required]
#endif
#if NET5_0_OR_GREATER
        [System.Text.Json.Serialization.JsonInclude]
#endif
        public IEnumerable<TItem> Items { get; protected set; }
    }
}
