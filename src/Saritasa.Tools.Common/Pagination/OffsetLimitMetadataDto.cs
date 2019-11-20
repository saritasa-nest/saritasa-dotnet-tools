// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Metadata data transfer object for offset limit list. Combines metadata and enumerable items.
    /// </summary>
    /// <typeparam name="TItem">Metadata items type.</typeparam>
#if NET40 || NET452 || NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
    [Serializable]
#endif
    public class OffsetLimitMetadataDto<TItem> : MetadataDto<TItem, OffsetLimitListMetadata>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">Metadata items.</param>
        /// <param name="metadata">Metadata object.</param>
        public OffsetLimitMetadataDto(IEnumerable<TItem> items, OffsetLimitListMetadata metadata) : base(items, metadata)
        {
        }

#if NET40 || NET452 || NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected OffsetLimitMetadataDto(
            System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        {
        }
#endif
    }
}
