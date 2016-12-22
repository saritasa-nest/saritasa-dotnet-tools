using System;
using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    /// <summary>
    /// Custom JsonConverter for RangeQuery
    /// </summary>
    public class RangeQuerySerializer : JsonConverter
    {
        public override bool CanWrite => true;

        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var range = value as RangeQuery;
            if (range == null || string.IsNullOrEmpty(range.Field))
            {
                return;
            }

            // write range as { "range" : { "Duration" : { "gt" : 100, "lt" : 200 } } } instead of { "Field": "Duration", "GreaterThan": 100, "LessThan": 200, ... }
            writer.WriteStartObject();
            writer.WritePropertyName("range");
            writer.WriteStartObject();
            writer.WritePropertyName(range.Field);
            writer.WriteStartObject();
            if (range.GreaterThan != null)
            {
                writer.WritePropertyName("gt");
                serializer.Serialize(writer, range.GreaterThan);
            }
            if (range.GreaterThanOrEqualTo != null)
            {
                writer.WritePropertyName("gte");
                serializer.Serialize(writer, range.GreaterThanOrEqualTo);
            }
            if (range.LessThan != null)
            {
                writer.WritePropertyName("lt");
                serializer.Serialize(writer, range.LessThan);
            }
            if (range.LessThanOrEqualTo != null)
            {
                writer.WritePropertyName("lte");
                serializer.Serialize(writer, range.LessThanOrEqualTo);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RangeQuery);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
