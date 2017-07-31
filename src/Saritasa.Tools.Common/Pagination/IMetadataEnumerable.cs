// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Enumerable that is able to return metadata.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <typeparam name="T">Enumerable type.</typeparam>
    public interface IMetadataEnumerable<out TMetadata, T> : IEnumerable<T>
    {
        /// <summary>
        /// Returns enumerable metadata as separate object.
        /// </summary>
        /// <returns>Metadata object.</returns>
        TMetadata GetMetadata();

        /// <summary>
        /// Return new object with metadata and data fields.
        /// </summary>
        /// <returns>Metadata and data object.</returns>
        MetadataDto<T> ToMetadataObject();

        /// <summary>
        /// Cast internal source type of metadata enumerable into another type
        /// using delegate. All existing metadata properties are kept.
        /// </summary>
        /// <typeparam name="TNew">New enumerable type.</typeparam>
        /// <param name="converter">Delegate to convert source enumerable fron existing type to new one.</param>
        /// <returns>Metadata enumerable with new type.</returns>
        IMetadataEnumerable<TMetadata, TNew> CastMetadataEnumerable<TNew>(Func<T, TNew> converter);
    }
}
