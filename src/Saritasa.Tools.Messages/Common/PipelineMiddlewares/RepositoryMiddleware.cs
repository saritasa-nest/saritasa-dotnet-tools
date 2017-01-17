// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    using System;
    using Abstractions;

    /// <summary>
    /// Saves the command execution context to repository.
    /// </summary>
    public class RepositoryMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; }

        readonly IMessageRepository repository;

        readonly RepositoryMessagesFilter filter;

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
            repository.AddAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(IMessage message)
        {
            if (filter != null && !filter.IsMatch(message))
            {
                return;
            }
            await repository.AddAsync(message).ConfigureAwait(false);
        }
    }
}
