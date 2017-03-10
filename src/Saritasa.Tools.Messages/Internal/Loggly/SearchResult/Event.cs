using Newtonsoft.Json;
using Saritasa.Tools.Messages.Common;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{
    public class EventMessage
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

    public class EventContent
    {
        [JsonProperty("json")]
        public Message Json { get; set; }

        [JsonProperty("http")]
        public Dictionary<string, string> Http { get; set; }
    }
}
