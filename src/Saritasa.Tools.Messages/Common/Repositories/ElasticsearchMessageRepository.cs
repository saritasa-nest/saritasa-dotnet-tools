// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NET40
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Threading;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common.ObjectSerializers;
using Saritasa.Tools.Messages.Internal.Elasticsearch.Query;
using Saritasa.Tools.Messages.Internal.Elasticsearch.SearchResult;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Use ElasticSearch to store messages.
    /// </summary>
    public class ElasticsearchMessageRepository : IMessageRepository, IDisposable
    {
        private const string KeyUrl = "uri";
        private const string ContentTypeJson = "application/json";

        /// <summary>
        /// Elasticsearch index.
        /// </summary>
        public string IndexName { get; private set; } = "saritasa-tools";

        /// <summary>
        /// Elasticsearch index type.
        /// </summary>
        public string IndexTypeName { get; private set; } = "messages";

        /// <summary>
        /// Elasticsearch web url.
        /// </summary>
        private readonly string uri;

        /// <summary>
        /// Json serializer.
        /// </summary>
        private readonly JsonObjectSerializer serializer;

        /// <summary>
        /// Client to be used to make queries to storage.
        /// </summary>
        private HttpClient client = new HttpClient();

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="uri">Uri to Elastictsearch service.</param>
        public ElasticsearchMessageRepository(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            this.uri = uri.TrimEnd('/');
            this.serializer = new JsonObjectSerializer();
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="uri">Uri to Elastictsearch service.</param>
        /// <param name="indexName">Index name.</param>
        /// <param name="indexType">Index type name.</param>
        public ElasticsearchMessageRepository(string uri, string indexName, string indexType)
            : this(uri)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }
            if (string.IsNullOrEmpty(indexType))
            {
                throw new ArgumentNullException(nameof(indexType));
            }
            IndexName = indexName;
            IndexTypeName = indexType;
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public ElasticsearchMessageRepository(IDictionary<string, string> parameters) :
            this(parameters[nameof(uri)])
        {
        }

        /// <inheritdoc />
        public async Task AddAsync(MessageRecord messageRecord, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            var id = messageRecord.Id.ToString("N");
            var response = await client
                .PutAsync(
                    $"{uri}/{IndexName}/{IndexTypeName}/{id}",
                    new StringContent(Encoding.UTF8.GetString(serializer.Serialize(messageRecord)),
                        Encoding.UTF8, ContentTypeJson),
                    cancellationToken
                )
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cannot insert message with id {id}: {await response.Content.ReadAsStringAsync()}.");
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            var searchQuery =
                new SearchQuery(CreateFieldQueries(messageQuery))
                    .WithFrom(messageQuery.Skip)
                    .WithSize(messageQuery.Take);

            var response = await client.PostAsync(
                $"{uri}/{IndexName}/{IndexTypeName}/_search",
                new StringContent(Encoding.UTF8.GetString(serializer.Serialize(searchQuery)),
                    Encoding.UTF8, ContentTypeJson),
                cancellationToken
            );
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cannot select messages: {await response.Content.ReadAsStringAsync()}.");
            }

            var result = await response.Content.ReadAsByteArrayAsync();
            var root = (Root)serializer.Deserialize(result, typeof(Root));
            var messages = root.Hits.Items.Select(x => x.Source).ToArray(); // message.Content, message.Error deserialized as JObject.
            foreach (var message in messages)
            {
                message.Content = Deserialize(message.Content.ToString(), Type.GetType(message.ContentType));
            }

            return messages;
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> parameters)
        {
            parameters[KeyUrl] = uri;
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

        private object Deserialize(string str, Type t)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var bytes = Encoding.UTF8.GetBytes(str);
            return serializer.Deserialize(bytes, t);
        }

        #region Dispose

        bool disposed;

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    client.Dispose();
                    client = null;
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

        #endregion
    }
}
#endif
