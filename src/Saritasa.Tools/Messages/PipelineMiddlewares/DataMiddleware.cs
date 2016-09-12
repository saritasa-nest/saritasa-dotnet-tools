// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.PipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using Messages;

    /// <summary>
    /// Appends additional data to message.
    /// </summary>
    public class DataMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "Data";

        Action<IDictionary<string, string>> action;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public DataMiddleware(Action<IDictionary<string, string>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            this.action = action;
        }

        /// <inheritdoc />
        public void Handle(Message context)
        {
            action(context.Data);
        }
    }
}
