// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using Abstractions;

    /// <summary>
    /// Dispose QueryObject of message if possible.
    /// </summary>
    public class QueryObjectReleaseMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; } = "Release";

        /// <inheritdoc />
        public void Handle(IMessage message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new ArgumentException("Message should be QueryMessage type");
            }

            // Release handler.
            var disposable = queryMessage.QueryObject as IDisposable;
            disposable?.Dispose();
            queryMessage.QueryObject = null;
        }
    }
}
