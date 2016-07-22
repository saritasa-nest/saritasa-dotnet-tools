// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages
{
    using System;

    /// <summary>
    /// Pipeline handler to process message. It may change message context.
    /// </summary>
    public interface IMessagePipelineMiddleware
    {
        /// <summary>
        /// Custom id field of handler. Just to identify it for debugging purposes.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Handles the message in context.
        /// </summary>
        /// <param name="message">Command execution context.</param>
        void Handle(Message message);
    }
}
