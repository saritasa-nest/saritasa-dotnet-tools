// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Pipeline handler to process message. It may change message context.
    /// </summary>
    public interface IMessagePipelineMiddleware
    {
        /// <summary>
        /// Middleware id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Handles the message in context.
        /// </summary>
        /// <param name="message">Message execution context.</param>
        void Handle([NotNull] IMessage message);
    }
}
