// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Common.ObjectSerializers
{
    /// <summary>
    /// Json object serializer based on NewtonJson library.
    /// </summary>
    public class JsonObjectSerializer : IObjectSerializer
    {
        static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        /// <inheritdoc />
        public object Deserialize(byte[] obj, Type type)
        {
            return JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(obj), type);
        }

        /// <inheritdoc />
        public byte[] Serialize(object obj)
        {
            var str = JsonConvert.SerializeObject(obj, Formatting.None, jsonSerializerSettings);
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        /// <inheritdoc />
        public bool IsText => true;
    }
}
