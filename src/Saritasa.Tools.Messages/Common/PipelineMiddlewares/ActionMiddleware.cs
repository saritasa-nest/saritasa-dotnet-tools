// Copyright (c) 2015-2017, Saritasa. All rights reserved.
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

        readonly Action<IMessage> action;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public ActionMiddleware(Action<IMessage> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            this.action = action;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessage context)
        {
            action(context);
        }
    }
}
