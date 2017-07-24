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
        private const string KeyActive = "active";
        private const string KeyRethrowExceptions = "rethrowexceptions";

        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// Is this middlware active. <c>True</c> by default.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Rethrow exceptions from repositories. <c>True</c> by default.
        /// </summary>
        public bool RethrowExceptions { get; set; } = true;

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
                throw new ArgumentException($"Parameters dictionary should contain {KeyRepositoryType} key.");
            }
            if (parameters.ContainsKey(KeyActive))
            {
                Active = Boolean.Parse(parameters[KeyActive]);
            }
            if (parameters.ContainsKey(KeyRethrowExceptions))
            {
                RethrowExceptions = Boolean.Parse(parameters[KeyRethrowExceptions]);
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
        public virtual void Handle(IMessageContext messageContext)
        {
            if (!Active)
            {
                return;
            }

            MessageRecord messageRecord = null;
            var convertPipeline = messageContext.Pipeline as IMessageRecordConverter;
            if (convertPipeline != null)
            {
                messageRecord = convertPipeline.CreateMessageRecord(messageContext);
            }
            else
            {
                messageRecord = new MessageRecord(messageContext);
            }

            if (filter != null && !filter.IsMatch(messageRecord))
            {
                return;
            }

            try
            {
                repository.AddAsync(messageRecord, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                if (RethrowExceptions)
                {
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            if (!Active)
            {
                return;
            }
            var messageRecord = new MessageRecord(messageContext);
            if (filter != null && !filter.IsMatch(messageRecord))
            {
                return;
            }
            try
            {
                await repository.AddAsync(messageRecord, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                if (RethrowExceptions)
                {
                    throw;
                }
            }
        }
    }
}
