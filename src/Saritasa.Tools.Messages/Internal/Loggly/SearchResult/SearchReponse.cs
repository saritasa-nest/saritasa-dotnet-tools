using Newtonsoft.Json;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{
    internal class SearchReponse
    {
        [JsonProperty("total_events")]
        public int Totals { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("rsid")]
        public Rsid Rsid { get; set; }

        [JsonProperty("events")]
        public List<EventMessage> Events { get; set; }
    }
}
