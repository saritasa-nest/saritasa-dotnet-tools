// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{
    internal class EventMessage
    {
        [JsonProperty("event")]
        public EventContent Item { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("logtypes")]
        public string[] LogTypes { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }
    }

    internal class EventContent
    {
        [JsonProperty("json")]
        public MessageRecord Json { get; set; }

        [JsonProperty("http")]
        public Dictionary<string, string> Http { get; set; }
    }
}
