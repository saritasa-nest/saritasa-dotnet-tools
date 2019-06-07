// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common.ObjectSerializers
{
    /// <summary>
    /// JSON object serializer based on NewtonJson library.
    /// </summary>
    public class JsonObjectSerializer : IObjectSerializer
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new SkipMetaPropertiesContractResolver()
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
