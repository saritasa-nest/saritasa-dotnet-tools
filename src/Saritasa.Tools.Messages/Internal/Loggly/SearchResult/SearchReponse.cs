using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{
    public class SearchReponse
    {
        [JsonProperty("rsid")]
        public Rsid Rsid { get; set; }
    }
}
