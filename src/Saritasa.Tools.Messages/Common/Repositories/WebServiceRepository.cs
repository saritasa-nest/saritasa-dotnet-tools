// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common.ObjectSerializers;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// The repository sends or receives messages to/from remote web endpoint.
    /// </summary>
    public class WebServiceRepository : IMessageRepository
    {
        private const string KeyUri = "uri";
        private const string DefaultEndpoint = @"http://localhost:9990/";
        private const string ContentTypeJson = "application/json";

        private HttpClient client = new HttpClient();
        private readonly string uri;
        private readonly JsonObjectSerializer serializer = new JsonObjectSerializer();

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebServiceRepository()
        {
            this.uri = DefaultEndpoint;
            Init();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="uri">URI to send messages.</param>
        public WebServiceRepository(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            this.uri = uri;
            Init();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public WebServiceRepository(IDictionary<string, string> parameters)
        {
            this.uri = parameters.GetValueOrInvoke(KeyUri, RepositoryConfigurationException.ThrowParameterNotExists);
            Init();
        }

        private void Init()
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        }

        #region IMessageRepository

        /// <inheritdoc />
        public async Task AddAsync(MessageRecord messageRecord, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            var content = Encoding.UTF8.GetString(serializer.Serialize(messageRecord));
            await client.PostAsync(
                uri,
                new StringContent(content, Encoding.UTF8, ContentTypeJson),
                cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }
            var queryString = GetQueryString(messageQuery);
            var geturi = new Uri(this.uri) + queryString;
            var httpResponse = await client.GetAsync(geturi, cancellationToken);
            var body = await httpResponse.Content.ReadAsStringAsync();

            var jarr = JArray.Parse(body);
            var list = new List<MessageRecord>();
            foreach (JToken jToken in jarr)
            {
                var jobj = jToken as JObject;
                if (jobj == null)
                {
                    continue;
                }
                var record = jobj.ToObject<MessageRecord>();
                if (record == null)
                {
                    continue;
                }

                if (record.Content is JObject)
                {
                    record.Content = ((JObject)record.Content).ToObject(Type.GetType(record.ContentType));
                }
                if (!string.IsNullOrEmpty(record.ErrorType) && jobj["Error"] is JObject)
                {
                    record.Error = (Exception)((JObject)jobj["Error"]).ToObject(Type.GetType(record.ErrorType));
                }

                list.Add(record);
            }
            return list;
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> parameters)
        {
            parameters[KeyUri] = uri;
        }

        #endregion

        private string GetQueryString(MessageQuery messageQuery)
        {
            var param = new List<string>();
            if (messageQuery.CreatedEndDate.HasValue)
            {
                param.Add($"endDate={messageQuery.CreatedEndDate.Value:yyyy-MM-dd}");
            }
            if (messageQuery.CreatedStartDate.HasValue)
            {
                param.Add($"startDate={messageQuery.CreatedStartDate.Value:yyyy-MM-dd}");
            }
            if (!string.IsNullOrEmpty(messageQuery.ContentType))
            {
                param.Add($"contentType={messageQuery.ContentType}");
            }
            if (!string.IsNullOrEmpty(messageQuery.ErrorType))
            {
                param.Add($"errorType={messageQuery.ErrorType}");
            }
            if (messageQuery.ExecutionDurationAbove.HasValue)
            {
                param.Add($"executionDurationAbove={messageQuery.ExecutionDurationAbove.Value}");
            }
            if (messageQuery.ExecutionDurationBelow.HasValue)
            {
                param.Add($"executionDurationBelow={messageQuery.ExecutionDurationBelow.Value}");
            }
            if (messageQuery.Id.HasValue)
            {
                param.Add($"id={messageQuery.Id}");
            }
            if (messageQuery.Status.HasValue)
            {
                param.Add($"status={messageQuery.Status}");
            }
            if (messageQuery.Skip > 0)
            {
                param.Add($"skip={messageQuery.Skip}");
            }
            if (messageQuery.Take > 0)
            {
                param.Add($"take={messageQuery.Take}");
            }
            return param.Count > 0 ? "?" + string.Join("&", param) : string.Empty;
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
