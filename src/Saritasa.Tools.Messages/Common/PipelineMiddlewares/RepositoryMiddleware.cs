// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// Saves the command execution context to repository.
    /// </summary>
    public class RepositoryMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        private const string KeyId = "id";
        private const string KeyRepositoryType = "repositorytype";

        /// <inheritdoc />
        public string Id { get; set; }

        private readonly IMessageRepository repository;

        private readonly RepositoryMessagesFilter filter;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public RepositoryMiddleware(IDictionary<string, string> parameters)
        {
            if (parameters.ContainsKey(KeyId))
            {
                Id = parameters[KeyId];
            }
            if (!parameters.ContainsKey(KeyRepositoryType))
            {
                throw new ArgumentException("Parameters dictionary should contain repositorytype key.");
            }

            this.repository = RepositoryFactory.CreateFromTypeName(parameters[KeyRepositoryType], parameters);
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="repository">Repository implementation.</param>
        public RepositoryMiddleware(IMessageRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
            this.repository = repository;
            Id = repository.GetType().Name;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="repository">Repository implementation.</param>
        /// <param name="filter">Filter incoming messages.</param>
        public RepositoryMiddleware(IMessageRepository repository, RepositoryMessagesFilter filter) : this(repository)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            this.filter = filter;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessage message)
        {
            if (filter != null && !filter.IsMatch(message))
            {
                return;
            }
            repository.AddAsync(message, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (filter != null && !filter.IsMatch(message))
            {
                return;
            }
            await repository.AddAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
