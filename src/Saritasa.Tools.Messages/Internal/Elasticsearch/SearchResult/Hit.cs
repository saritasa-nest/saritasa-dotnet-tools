// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.SearchResult
{
    internal class Hit
    {
        [JsonProperty(PropertyName = "_index")]
        public string Index { get; set; }

        [JsonProperty(PropertyName = "_type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "_score")]
        public string Score { get; set; }

        [JsonProperty(PropertyName = "_source")]
        public MessageRecord Source { get; set; }
    }
}
