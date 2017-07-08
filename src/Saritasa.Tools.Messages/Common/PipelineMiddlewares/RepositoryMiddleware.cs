// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
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
        /// <param name="dict">Parameters dictionary.</param>
        public RepositoryMiddleware(IDictionary<string, string> dict)
        {
            if (dict.ContainsKey("id"))
            {
                Id = dict["id"];
            }
            if (!dict.ContainsKey("repositoryType"))
            {
                throw new ArgumentException("Parameters dictionary should contain repositoryType key.");
            }

            var repositoryTypeName = dict["repositoryType"];
            var repositoryType = Type.GetType(repositoryTypeName);
            if (repositoryType == null)
            {
                throw new ArgumentException($"Cannot load repository type {repositoryTypeName}.");
            }

            var ctor = repositoryType.GetTypeInfo().GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1
                                                                  && c.GetParameters()[0].ParameterType == typeof(IDictionary<string, string>));
            if (ctor == null)
            {
                ctor = repositoryType.GetTypeInfo().GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
            }

            if (ctor == null)
            {
                var msg = "Cannot find public parameterless constructor or constructor that accepts IDictionary<string, string>.";
                throw new InvalidOperationException(msg);
            }

            var repository = ctor.GetParameters().Length == 1
                ? ctor.Invoke(new object[] { dict }) as IMessageRepository
                : ctor.Invoke(new object[] { }) as IMessageRepository;
            if (repository == null)
            {
                throw new InvalidOperationException($"Cannot instaniate repository {repositoryType.Name}.");
            }
            this.repository = repository;
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
