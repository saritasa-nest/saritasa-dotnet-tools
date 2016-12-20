using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    [JsonConverter(typeof(RangeQuerySerializer))]
    public class RangeQuery : IFieldNameQuery
    {
        public string Field { get; set; }

        public object GreaterThanOrEqualTo { get; set; }

        public object LessThanOrEqualTo { get; set; }

        public object GreaterThan { get; set; }

        public object LessThan { get; set; }
    }
}
