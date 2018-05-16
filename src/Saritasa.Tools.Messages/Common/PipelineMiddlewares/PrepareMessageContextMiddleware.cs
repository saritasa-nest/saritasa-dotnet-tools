// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// The middleware prepares message context for message processing. It fills ContentId field.
    /// </summary>
    public class PrepareMessageContextMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        /// <summary>
        /// Middleware identifier.
        /// </summary>
        public string Id => nameof(PrepareMessageContextMiddleware);

        /// <inheritdoc />
        public Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (messageContext.Content == null)
            {
                throw new InvalidOperationException("Message content cannot be null.");
            }
            messageContext.ContentId = TypeHelpers.GetPartiallyAssemblyQualifiedName(messageContext.Content.GetType());
            return Task.FromResult(1);
        }

        /// <inheritdoc />
        public void Handle(IMessageContext messageContext)
        {
            if (messageContext.Content == null)
            {
                throw new InvalidOperationException("Message content cannot be null.");
            }
            messageContext.ContentId = TypeHelpers.GetPartiallyAssemblyQualifiedName(messageContext.Content.GetType());
        }
    }
}
