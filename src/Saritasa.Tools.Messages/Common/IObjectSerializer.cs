// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Common
{
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
        byte[] Serialize([NotNull] object obj);

        /// <summary>
        /// Deserialized bytes to object of given type.
        /// </summary>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Object.</returns>
        object Deserialize([NotNull] byte[] bytes, Type type);

        /// <summary>
        /// Does serializer represent text string. Binary by default.
        /// </summary>
        bool IsText { get; }
    }
}
