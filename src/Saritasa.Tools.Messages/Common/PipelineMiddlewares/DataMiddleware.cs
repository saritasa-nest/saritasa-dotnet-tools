// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// Appends additional data to message.
    /// </summary>
    public class DataMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "Data";

        readonly Action<IDictionary<object, object>> action;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public DataMiddleware(Action<IDictionary<object, object>> action)
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
            action(messageContext.Items);
        }
    }
}
