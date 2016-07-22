// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
namespace Saritasa.Tools.Messages.ObjectSerializers
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Object binary serializer.
    /// </summary>
    public class BinaryObjectSerializer : IObjectSerializer
    {
        /// <inheritdoc />
        public object Deserialize(byte[] bytes, Type type)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (var memory = new MemoryStream(bytes))
            {
                return formatter.Deserialize(memory);
            }
        }

        /// <inheritdoc />
        public byte[] Serialize(object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (var memory = new MemoryStream())
            {
                formatter.Serialize(memory, obj);
                return memory.ToArray();
            }
        }

        /// <inheritdoc />
        public bool IsText
        {
            get { return false; }
        }
    }
}
#endif
