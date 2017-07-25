// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common.ObjectSerializers;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Send or get messages from remote web endpoint.
    /// </summary>
    public class WebServiceRepository : IMessageRepository
    {
        private const string KeyUri = "uri";
        private const string DefaultEndpoint = @"http://localhost:9990/";

        private HttpClient client = new HttpClient();
        private readonly string uri;
        private readonly JsonObjectSerializer serializer = new JsonObjectSerializer();

        /// <summary>
        /// .ctor
        /// </summary>
        public WebServiceRepository()
        {
            this.uri = DefaultEndpoint;
            Init();
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="uri">Uri to send messages.</param>
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
        /// .ctor
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public WebServiceRepository([NotNull] IDictionary<string, string> parameters)
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
                new StringContent(content, Encoding.UTF8, "application/json"),
                cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> parameters)
        {
            parameters[KeyUri] = uri;
        }

        #endregion

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
