// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Text;
    using Abstractions;
    using ObjectSerializers;
    using Newtonsoft.Json;
    using Internal.Loggly.SearchResult;
    using System.Linq;

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

        HttpClient client = new HttpClient();

        readonly string token;
        readonly string username = "";
        readonly string password = "";
        readonly string accountDomain = "";

        /// <summary>
        /// Json serializer.
        /// </summary>
        private readonly JsonObjectSerializer serializer = new JsonObjectSerializer();

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="token">Customer token.</param>
        /// <param name="username">Customer username.</param>
        /// <param name="password">Customer password.</param>
        /// <param name="accountDomain">Customer domain name</param>
        public LogglyMessageRepository(string token, string username = "", string password = "", string accountDomain = "")
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            this.token = token;
            this.client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(new ProductHeaderValue("SaritasaTools")));

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(accountDomain))
            {
                this.username = username;
                this.password = password;
                this.accountDomain = accountDomain;

                this.client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", this.username, this.password))));
            }
        }

        /// <inheritdoc />
        public async Task AddAsync(IMessage message)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            var content = Encoding.UTF8.GetString(serializer.Serialize(((Message)message).CloneToMessage()));
            await client
                .PostAsync($"{ServerEndpoint}/inputs/{token}",
                    new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IMessage>> GetAsync(string searchQuery)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            if (client.DefaultRequestHeaders.Authorization == null)
            {
                throw new ArgumentNullException(nameof(username) + nameof(password) + nameof(accountDomain));
            }

            // Get search result
            var searchResponse = await client.GetAsync(string.Format(SearchEndpoint, accountDomain, searchQuery));
            if (searchResponse.IsSuccessStatusCode)
            {
                var searchResponseBody = (SearchReponse)serializer.Deserialize(await searchResponse.Content.ReadAsByteArrayAsync(), typeof(SearchReponse));

                // Get event result
                var eventResponse = await client.GetAsync(string.Format(RetrievingEventsEndpoint, accountDomain, searchResponseBody.Rsid.ToString()));
                if (eventResponse.IsSuccessStatusCode)
                {
                    return (EventResponse[])serializer.Deserialize(await eventResponse.Content.ReadAsByteArrayAsync(), typeof(EventResponse[]));
                }
            }

            return null;
        }

        /// <summary>
        /// Search for events
        /// </summary>
        /// <param name="messageQuery">
        /// ContentType         : query string, check out https://www.loggly.com/docs/search-query-language/
        /// CreatedStartDate    : Start time for the search.
        /// CreatedEndDate      : End time for the search.
        /// Order               : Direction of results returned, either "asc" or "desc". Defaults to "desc"
        /// Take                : number of rows returned by search
        /// </param>
        /// <returns>array of message</returns>
        public async Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            return await GetAsync(CreateQueryString(messageQuery));
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            dict[nameof(token)] = token;
            dict[nameof(username)] = username;
            dict[nameof(password)] = password;
            dict[nameof(accountDomain)] = accountDomain;
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Loggly repository.</returns>
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
            return new LogglyMessageRepository(
                dict[nameof(token)].ToString(),
                dict[nameof(username)].ToString(),
                dict[nameof(password)].ToString(),
                dict[nameof(accountDomain)].ToString()
            );
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
        /// Convert MessageQuery object to query string
        /// </summary>
        /// <param name="messageQuery"></param>
        /// <returns>query string</returns>
        private string CreateQueryString(MessageQuery messageQuery)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            if (messageQuery.CreatedStartDate.HasValue)
            {
                dic.Add("from", messageQuery.CreatedStartDate.Value.ToString("yyyy-MM-dd HH:mm:ss.SSS"));
            }
            if (messageQuery.CreatedEndDate.HasValue)
            {
                dic.Add("until", messageQuery.CreatedEndDate.Value.ToString("yyyy-MM-dd HH:mm:ss.SSS"));
            }
            dic.Add("q", messageQuery.Query);
            dic.Add("order", messageQuery.Order == Order.Ascending ? "asc" : "desc");
            dic.Add("size", messageQuery.Take.ToString());

            return string.Join("&", dic.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
        }
    }
}
