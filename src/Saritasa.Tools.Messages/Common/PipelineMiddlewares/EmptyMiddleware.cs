// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// Middleware that does nothing. Can be useful for example when you need to replace it with another
    /// useful middleware after initialization.
    /// </summary>
    public class EmptyMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        /// <summary>
        /// Middleware identifier.
        /// </summary>
        public string Id => nameof(EmptyMiddleware);

        /// <inheritdoc />
        public Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(1);
        }

        /// <inheritdoc />
        public void Handle(IMessageContext messageContext)
        {
        }
    }
}
