// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.SearchResult
{
    internal class Shards
    {
        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; }

        [JsonProperty(PropertyName = "successful")]
        public string Successful { get; set; }

        [JsonProperty(PropertyName = "failed")]
        public string Failed { get; set; }
    }
}
