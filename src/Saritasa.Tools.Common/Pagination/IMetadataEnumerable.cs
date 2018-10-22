// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Enumerable that is able to return its metadata.
    /// </summary>
    /// <typeparam name="TItem">Enumerable item type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    public interface IMetadataEnumerable<TItem, TMetadata> : IEnumerable<TItem>
    {
        /// <summary>
        /// Return new object with metadata and data fields.
        /// </summary>
        /// <returns>Metadata and data object.</returns>
        MetadataDto<TItem, TMetadata> ToMetadataObject();

        /// <summary>
        /// Cast internal source type of metadata enumerable into another type
        /// using delegate. All existing metadata properties are kept.
        /// </summary>
        /// <typeparam name="TItemNew">New enumerable type.</typeparam>
        /// <param name="converter">Delegate to convert source enumerable from existing type to new one.</param>
        /// <returns>Metadata enumerable with new type.</returns>
        IMetadataEnumerable<TItemNew, TMetadata> Convert<TItemNew>(
            Func<TItem, TItemNew> converter);
    }
}
