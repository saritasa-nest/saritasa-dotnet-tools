// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NET40
namespace Saritasa.Tools.Messages.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Abstractions;
    using ObjectSerializers;
    using Internal.Elasticsearch.Query;
    using Internal.Elasticsearch.SearchResult;

    /// <summary>
    /// Use ElasticSearch to store messages.
    /// </summary>
    public class ElasticsearchMessageRepository : IMessageRepository, IDisposable
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
            this.client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(new ProductHeaderValue("SaritasaTools")));
        }

        /// <inheritdoc />
        public async Task AddAsync(IMessage message)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            await client
                .PutAsync($"{uri}/{IndexName}/{IndexTypeName}/{message.Id}",
                    new ByteArrayContent(serializer.Serialize(((Message)message).CloneToMessage())))
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

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
            return new ElasticsearchMessageRepository(dict[nameof(uri)].ToString());
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
