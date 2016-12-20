using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    [JsonConverter(typeof(TermQuerySerializer))]
    public class TermQuery : IFieldNameQuery
    {
        public string Field { get; set; }

        public object Value { get; set; }
    }
}
