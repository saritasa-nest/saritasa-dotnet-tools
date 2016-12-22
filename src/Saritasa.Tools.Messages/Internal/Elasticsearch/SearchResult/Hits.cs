using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.SearchResult
{
    internal class Hits
    {
        public Hits()
        {
            this.Items = new Hit[0];
        }

        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; }

        [JsonProperty(PropertyName = "max_score")]
        public string MaxScore { get; set; }

        [JsonProperty(PropertyName = "hits")]
        public Hit[] Items { get; set; }
    }
}
