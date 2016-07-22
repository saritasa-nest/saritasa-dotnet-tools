// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages
{
    using System;

    /// <summary>
    /// Simple interface to serialize/deserialize POCOs.
    /// </summary>
    public interface IObjectSerializer
    {
        /// <summary>
        /// Serialize object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Serialized to bytes object.</returns>
        byte[] Serialize(object obj);

        /// <summary>
        /// Deserialized bytes to object of given type.
        /// </summary>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Object.</returns>
        object Deserialize(byte[] bytes, Type type);

        /// <summary>
        /// Is current serializer represents text string.
        /// If not binary is meant by default.
        /// </summary>
        bool IsText { get; }
    }
}
