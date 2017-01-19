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

    /// <summary>
    /// Loggly service to store messages. See more at http://www.loggly.com . You can
    /// issue API token at Source Setup -> Customer Tokens section of website.
    /// </summary>
    /// <remarks>
    /// Information about HTTP endpoint: https://www.loggly.com/docs/http-endpoint/
    /// </remarks>
    public class LogglyMessageRepository : IMessageRepository, IDisposable
    {
        const string ServerEndpoint = @"https://logs-01.loggly.com";
        const string TagsHeader = "X-LOGGLY-TAG";

        HttpClient client = new HttpClient();

        readonly string token;

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
        public Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            dict[nameof(token)] = token;
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Loggly repository.</returns>
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
            return new LogglyMessageRepository(dict[nameof(token)].ToString());
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
