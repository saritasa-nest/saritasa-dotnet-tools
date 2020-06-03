// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Metadata data transfer object for total count list. Combines metadata and enumerable items.
    /// </summary>
    /// <typeparam name="TItem">Metadata items type.</typeparam>
#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
    [Serializable]
#endif
    public class TotalCountListMetadataDto<TItem> : MetadataDto<TItem, TotalCountListMetadata>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">Metadata items.</param>
        /// <param name="metadata">Metadata object.</param>
        public TotalCountListMetadataDto(IEnumerable<TItem> items, TotalCountListMetadata metadata) : base(items, metadata)
        {
        }

#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected TotalCountListMetadataDto(
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        {
        }
#endif
    }
}
