using System;
using Newtonsoft.Json;

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    /// <summary>
    /// Custom JsonConverter for TermQuery
    /// </summary>
    public class TermQuerySerializer : JsonConverter
    {
        public override bool CanWrite => true;

        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var term = value as TermQuery;
            if (term == null || string.IsNullOrEmpty(term.Field) || term.Value == null)
            {
                return;
            }

            // write term as { "term": { "_id": 123 } } instead of { "Field": "_id", "Value": 123 }
            writer.WriteStartObject();
            writer.WritePropertyName("term");
            writer.WriteStartObject();
            writer.WritePropertyName(term.Field);
            serializer.Serialize(writer, term.Value);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TermQuery);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
