using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Loggly.SearchResult
{

    [JsonObject]
    public class Rsid
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("date_from")]
        public long From { get; set; }

        [JsonProperty("date_to")]
        public long To { get; set; }

        [JsonProperty("elapsed_time")]
        public double ElapsedTime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return string.Format("rsid={0}", Id);
        }
    }
}
