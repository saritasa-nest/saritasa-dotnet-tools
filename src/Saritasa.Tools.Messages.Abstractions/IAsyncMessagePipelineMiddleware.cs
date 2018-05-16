// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Async pipeline handler to process message. It may change message context.
    /// </summary>
    public interface IAsyncMessagePipelineMiddleware
    {
        /// <summary>
        /// Middleware identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Handles the message in context asynchronously.
        /// </summary>
        /// <param name="messageContext">Message execution context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken =
            default(CancellationToken));
    }
}
