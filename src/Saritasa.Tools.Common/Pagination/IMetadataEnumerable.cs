// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Enumerable that is able to return metadata.
    /// </summary>
    /// <typeparam name="T">Metadata type.</typeparam>
    public interface IMetadataEnumerable<out T>
    {
        /// <summary>
        /// Returns enumerable metadata as separate object.
        /// </summary>
        /// <returns>Metadata object.</returns>
        T GetMetadata();
    }
}
