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
