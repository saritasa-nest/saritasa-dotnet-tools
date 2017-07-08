// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    /// <summary>
    /// Dispose QueryObject of message if possible.
    /// </summary>
    public class QueryObjectReleaseMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; } = "Release";

        /// <inheritdoc />
        public virtual void Handle(IMessage message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(QueryMessage)));
            }

            // Release handler.
            var disposable = queryMessage.QueryObject as IDisposable;
            disposable?.Dispose();
            queryMessage.QueryObject = null;
        }
    }
}
