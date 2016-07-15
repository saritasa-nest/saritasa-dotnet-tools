// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.ObjectSerializers
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Json object serializer based on Newton Json lib.
    /// </summary>
    public class JsonObjectSerializer : IObjectSerializer
    {
        /// <inheritdoc />
        public object Deserialize(byte[] obj, Type type)
        {
            return JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(obj), type);
        }

        /// <inheritdoc />
        public byte[] Serialize(object obj)
        {
            var str = JsonConvert.SerializeObject(obj, Formatting.None);
            return System.Text.Encoding.UTF8.GetBytes(str);
        }
    }
}
