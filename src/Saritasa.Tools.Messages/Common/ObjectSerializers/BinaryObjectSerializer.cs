// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#if !NETCOREAPP1_1 && !NETSTANDARD1_6
namespace Saritasa.Tools.Messages.Common.ObjectSerializers
{
    /// <summary>
    /// Object binary serializer.
    /// </summary>
    public class BinaryObjectSerializer : IObjectSerializer
    {
        /// <inheritdoc />
        public object Deserialize(byte[] bytes, Type type)
        {
            var formatter = new BinaryFormatter();
            using (var memory = new MemoryStream(bytes))
            {
                return formatter.Deserialize(memory);
            }
        }

        /// <inheritdoc />
        public byte[] Serialize(object obj)
        {
            var formatter = new BinaryFormatter();
            using (var memory = new MemoryStream())
            {
                formatter.Serialize(memory, obj);
                return memory.ToArray();
            }
        }

        /// <inheritdoc />
        public bool IsText => false;
    }
}
#endif
