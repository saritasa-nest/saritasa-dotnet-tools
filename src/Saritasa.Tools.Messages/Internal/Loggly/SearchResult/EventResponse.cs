using Newtonsoft.Json;
using Saritasa.Tools.Messages.Common;
using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{
    public class EventResponse : Message
    {
        [JsonProperty("event")]
        public override IDictionary<string, string> Data { get; set; }

        [JsonProperty("timestamp")]
        public override DateTime CreatedAt { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("logtypes")]
        public string[] LogTypes { get; set; }
    }
}
