// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NET40
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common.ObjectSerializers;
using Saritasa.Tools.Messages.Internal.Loggly.SearchResult;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Loggly service to store messages. See more at http://www.loggly.com . You can
    /// issue API token at Source Setup -> Customer Tokens section of website.
    /// </summary>
    /// <remarks>
    /// Information about HTTP endpoint: https://www.loggly.com/docs/http-endpoint/
    /// </remarks>
    public class LogglyMessageRepository : IMessageRepository, IDisposable
    {
        const string SearchEndpoint = "https://{0}.loggly.com/apiv2/search?{1}";
        const string RetrievingEventsEndpoint = "https://{0}.loggly.com/apiv2/events?{1}";
        const string ServerEndpoint = @"https://logs-01.loggly.com";
        const string TagsHeader = "X-LOGGLY-TAG";

        private HttpClient client = new HttpClient();

        private readonly string token;
        private readonly string username;
        private readonly string password;
        private readonly string accountDomain;

        /// <summary>
        /// Json serializer.
        /// </summary>
        private readonly JsonObjectSerializer serializer = new JsonObjectSerializer();

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="token">Customer token.</param>
        public LogglyMessageRepository(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            this.token = token;
            this.client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(new ProductHeaderValue("SaritasaTools")));
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="token">Customer token.</param>
        /// <param name="username">Customer username.</param>
        /// <param name="password">Customer password.</param>
        /// <param name="accountDomain">Customer domain name.</param>
        public LogglyMessageRepository(string token, string username, string password, string accountDomain) : this(token)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (string.IsNullOrEmpty(accountDomain))
            {
                throw new ArgumentNullException(nameof(accountDomain));
            }

            this.username = username;
            this.password = password;
            this.accountDomain = accountDomain;
            this.client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(string.Format("{0}:{1}", this.username, this.password))));
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        public LogglyMessageRepository(IDictionary<string, string> dict)
        {
            var repositoryUsername = dict[nameof(username)].ToString();

            if (!string.IsNullOrEmpty(repositoryUsername))
            {
                if (!dict.ContainsKey(nameof(token)) || !dict.ContainsKey(nameof(username))
                    || !dict.ContainsKey(nameof(password)) || !dict.ContainsKey(nameof(accountDomain)))
                {
                    throw new ArgumentException($"No required parameters: {nameof(token)}," +
                                                $"{nameof(username)}, {nameof(password)}, {nameof(accountDomain)}", nameof(dict));
                }
                this.token = dict[nameof(token)];
                this.username = dict[nameof(username)];
                this.password = dict[nameof(password)];
                this.accountDomain = dict[nameof(accountDomain)];
            }
            else
            {
                if (!dict.ContainsKey(nameof(token)))
                {
                    throw new ArgumentException($"No required parameter: {nameof(token)}", nameof(dict));
                }
                this.token = dict[nameof(token)];
            }
        }

        /// <inheritdoc />
        public async Task AddAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            var content = Encoding.UTF8.GetString(serializer.Serialize(((Message)message).CloneToMessage()));
            await client
                .PostAsync(
                    $"{ServerEndpoint}/inputs/{token}",
                    new StringContent(content, Encoding.UTF8, "application/json"),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Search for events.
        /// </summary>
        /// <param name="messageQuery">Message query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <remarks>
        /// Source: https://www.loggly.com/docs/search-query-language/
        /// </remarks>
        /// <returns>Enumerable of found messages.</returns>
        public async Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            if (client.DefaultRequestHeaders.Authorization == null)
            {
                throw new InvalidOperationException(nameof(username) + nameof(password) + nameof(accountDomain));
            }

            // Get rsid.
            string rsId = await CallSearchApi(CreateQueryString(messageQuery), cancellationToken);

            // Get event result. If Skip > 0, we need to start from the second page, else start from the first page.
            var eventResponse = await CallEventApi(rsId, messageQuery.Skip > 0 ? 1 : 0);
            var response = (SearchReponse)serializer.Deserialize(
                await eventResponse.Content.ReadAsByteArrayAsync(), typeof(SearchReponse));

            return response.Events.Select(e => e.Item.Json).ToArray();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> dict)
        {
            dict[nameof(token)] = token;
            dict[nameof(username)] = username;
            dict[nameof(password)] = password;
            dict[nameof(accountDomain)] = accountDomain;
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

        /// <summary>
        /// Call event api to get event list.
        /// </summary>
        /// <param name="rsId">Rsid.</param>
        /// <param name="page">Page number.</param>
        /// <returns></returns>
        private Task<HttpResponseMessage> CallEventApi(string rsId, int page)
        {
            return client.GetAsync(string.Format(RetrievingEventsEndpoint, accountDomain, $"rsid={rsId}&page={page}"));
        }

        /// <summary>
        /// Call search api to init event api.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>RsId.</returns>
        private async Task<string> CallSearchApi(string query, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync(string.Format(SearchEndpoint, accountDomain, query), cancellationToken);
            var content = (SearchReponse)serializer.Deserialize(
                await response.Content.ReadAsByteArrayAsync(), typeof(SearchReponse));
            return content.Rsid.Id;
        }

        /// <summary>
        /// Convert MessageQuery object to query string.
        /// </summary>
        /// <param name="messageQuery"></param>
        /// <returns>Query string.</returns>
        private string CreateQueryString(MessageQuery messageQuery)
        {
            var dic = new Dictionary<string, string>();
            var query = new List<string>();

            if (messageQuery.CreatedStartDate.HasValue)
            {
                dic.Add("from", messageQuery.CreatedStartDate.Value.ToString("s"));
            }
            if (messageQuery.CreatedEndDate.HasValue)
            {
                dic.Add("until", messageQuery.CreatedEndDate.Value.ToString("s"));
            }

            // Order is not implemented in other repositories. Make desc by default.
            dic.Add("order", "desc");
            dic.Add("size", messageQuery.Take.ToString());

            // Build query string.
            if (messageQuery.Id != null)
            {
                query.Add($"json.Id:\"{messageQuery.Id}\"");
            }
            if (!string.IsNullOrEmpty(messageQuery.ContentType))
            {
                query.Add($"json.ContentType:\"{messageQuery.ContentType}\"");
            }
            if (!string.IsNullOrEmpty(messageQuery.ErrorType))
            {
                query.Add($"json.ErrorType:\"{messageQuery.ErrorType}\"");
            }
            if (messageQuery.Status != null)
            {
                query.Add($"json.Status:\"{(int)messageQuery.Status}\"");
            }
            if (messageQuery.Type != null)
            {
                query.Add($"json.Type:\"{messageQuery.Type}\"");
            }
            dic.Add("q", query.Count > 0 ? string.Join(" AND ", query.ToArray()) : "*");

            return string.Join("&", dic.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
        }
    }
}
#endif
