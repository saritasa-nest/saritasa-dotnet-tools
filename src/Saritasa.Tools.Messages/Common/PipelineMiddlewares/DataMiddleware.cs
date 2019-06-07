// Copyright (c) 2015-2019 Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// Appends additional data to message.
    /// </summary>
    public class DataMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; }

        private readonly Action<IDictionary<string, string>> action;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        public DataMiddleware(Action<IDictionary<string, string>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            this.action = action;
            this.Id = this.GetType().Name;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            var obj = messageContext.Items.GetValueOrDefault(MessageContextConstants.DataKey,
                new Dictionary<string, string>());
            if (obj is IDictionary<string, string> data)
            {
                action(data);
            }
        }
    }
}
