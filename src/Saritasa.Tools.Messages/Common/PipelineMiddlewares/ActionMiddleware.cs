// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// Execute user action on every message.
    /// </summary>
    public class ActionMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "Action";

        readonly Action<IMessageContext> action;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public ActionMiddleware(Action<IMessageContext> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            this.action = action;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            action(messageContext);
        }
    }
}
