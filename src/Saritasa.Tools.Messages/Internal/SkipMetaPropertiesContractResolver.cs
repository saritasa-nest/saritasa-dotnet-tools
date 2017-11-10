// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// JSON contract resolver that skips serializing of properties that does not provide
    /// useful information or requires a lot of size.
    /// </summary>
    internal class SkipMetaPropertiesContractResolver : DefaultContractResolver
    {
        private static readonly string[] propertiesToSkip = { "WatsonBuckets", "TargetSite" };

        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (propertiesToSkip.Contains(property.PropertyName))
            {
                property.Ignored = true;
            }
            return property;
        }
    }
}
