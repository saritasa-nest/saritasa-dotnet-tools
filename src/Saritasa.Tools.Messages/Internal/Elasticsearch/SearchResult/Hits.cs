// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.SearchResult
{
    internal class Hits
    {
        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; }

        [JsonProperty(PropertyName = "max_score")]
        public string MaxScore { get; set; }

        [JsonProperty(PropertyName = "hits")]
        public Hit[] Items { get; set; } = new Hit[0];
    }
}
