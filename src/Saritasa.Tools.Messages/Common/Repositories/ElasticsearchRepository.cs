// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Abstractions;
    using ObjectSerializers;
    using Internal.Elasticsearch.Query;
    using Internal.Elasticsearch.SearchResult;

    /// <summary>
    /// Use ElasticSearch to store messages.
    /// </summary>
    public class ElasticsearchRepository : IMessageRepository, IDisposable
    {
        /// <summary>
        /// Elasticsearch index.
        /// </summary>
        private const string IndexName = "saritasa";

        /// <summary>
        /// Elasticsearch index type.
        /// </summary>
        private const string IndexTypeName = "messages";

        /// <summary>
        /// Elasticsearch web url.
        /// </summary>
        private readonly string uri;

        /// <summary>
        /// JsonSerializer
        /// </summary>
        private readonly JsonObjectSerializer serializer;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="uri">Uri to Elastictsearch service.</param>
        public ElasticsearchRepository(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            this.uri = uri.TrimEnd('/');
            this.serializer = new JsonObjectSerializer();
        }

        /// <inheritdoc />
        public void Add(IMessage message)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }
            SaveMessageAsync((Message)message);
        }

        /// <inheritdoc />
        public IEnumerable<IMessage> Get(MessageQuery messageQuery)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }
            return GetAsync(messageQuery).Result;
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            dict[nameof(uri)] = uri;
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Elasticsearch repository.</returns>
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
            return new ElasticsearchRepository(dict[nameof(uri)].ToString());
        }

        private HttpClient client = new HttpClient();

        private void SaveMessageAsync(Message message)
        {
            //using (var client = new HttpClient())
            {
                client
                    .PutAsync($"{uri}/{IndexName}/{IndexTypeName}/{message.Id}",
                        new ByteArrayContent(serializer.Serialize(message.CloneToMessage())))
                    .ConfigureAwait(false)
                    .GetAwaiter().GetResult();
            }
        }

        private async Task<IEnumerable<Message>> GetAsync(MessageQuery messageQuery)
        {
            using (var client = new HttpClient())
            {
                var searchQuery =
                    new SearchQuery(CreateFieldQueries(messageQuery))
                        .WithFrom(messageQuery.Skip)
                        .WithSize(messageQuery.Take);

                var response = await client.PostAsync($"{uri}/{IndexName}/{IndexTypeName}/_search",
                    new ByteArrayContent(serializer.Serialize(searchQuery)));

                var result = await response.Content.ReadAsByteArrayAsync();
                var root = (Root)serializer.Deserialize(result, typeof(Root));
                var messages = root.Hits.Items.Select(x => x.Source).ToArray(); // message.Content deserialized as JObject

                return messages;
            }
        }

        private IEnumerable<IFieldNameQuery> CreateFieldQueries(MessageQuery messageQuery)
        {
            var filterQueries = new List<IFieldNameQuery>();
            if (messageQuery.Id.HasValue)
            {
                filterQueries.Add(new TermQuery { Field = "_id", Value = messageQuery.Id.Value });
            }
            if (!string.IsNullOrEmpty(messageQuery.ContentType))
            {
                filterQueries.Add(new TermQuery { Field = "ContentType", Value = messageQuery.ContentType.ToLowerInvariant() });
            }
            if (!string.IsNullOrEmpty(messageQuery.ErrorType))
            {
                filterQueries.Add(new TermQuery { Field = "ErrorType", Value = messageQuery.ErrorType.ToLowerInvariant() });
            }
            if (messageQuery.Status.HasValue)
            {
                filterQueries.Add(new TermQuery { Field = "Status", Value = messageQuery.Status.Value });
            }
            if (messageQuery.Type.HasValue)
            {
                filterQueries.Add(new TermQuery { Field = "Type", Value = messageQuery.Type.Value });
            }
            if (messageQuery.CreatedStartDate.HasValue || messageQuery.CreatedEndDate.HasValue)
            {
                filterQueries.Add(new RangeQuery
                {
                    Field = "CreatedAt",
                    GreaterThanOrEqualTo = messageQuery.CreatedStartDate,
                    LessThanOrEqualTo = messageQuery.CreatedEndDate
                });
            }
            if (messageQuery.ExecutionDurationAbove.HasValue || messageQuery.ExecutionDurationBelow.HasValue)
            {
                filterQueries.Add(new RangeQuery
                {
                    Field = "ExecutionDuration",
                    GreaterThanOrEqualTo = messageQuery.ExecutionDurationAbove,
                    LessThanOrEqualTo = messageQuery.ExecutionDurationBelow
                });
            }
            return filterQueries;
        }

        bool disposed;

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //Close();
                }
                disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
