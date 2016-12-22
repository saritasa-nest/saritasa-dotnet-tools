// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal.Elasticsearch.Query
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Custom JsonConverter for SearchQuery.
    /// </summary>
    internal class SearchQuerySerializer : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var query = value as SearchQuery;
            if (query == null)
            {
                return;
            }

            // write query in Elasticsearch format
            writer.WriteStartObject(); // root

            writer.WritePropertyName("from");
            serializer.Serialize(writer, query.From);

            writer.WritePropertyName("size");
            serializer.Serialize(writer, query.Size);

            writer.WritePropertyName("query");
            writer.WriteStartObject(); // query

            if (query.FieldQueries != null && query.FieldQueries.Any())
            {
                writer.WritePropertyName("constant_score");
                writer.WriteStartObject(); // constant_score

                writer.WritePropertyName("filter");
                writer.WriteStartObject(); // filter

                writer.WritePropertyName("bool");
                writer.WriteStartObject(); // bool

                writer.WritePropertyName("must");
                serializer.Serialize(writer, query.FieldQueries);

                writer.WriteEndObject(); // of bool
                writer.WriteEndObject(); // of filter
                writer.WriteEndObject(); // of constant_score
            }
            else
            {
                // write match_all
                writer.WritePropertyName("match_all");
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            writer.WriteEndObject(); // of query
            writer.WriteEndObject(); // of root
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SearchQuery);
        }
    }
}
