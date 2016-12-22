// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Custom JsonConverter for TermQuery.
    /// </summary>
    internal class TermQuerySerializer : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var term = value as TermQuery;
            if (string.IsNullOrEmpty(term?.Field) || term.Value == null)
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

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TermQuery);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
