// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Events
{
    /// <summary>
    /// Events pipeline.
    /// </summary>
    public class EventPipeline : MessagePipeline, IEventPipeline
    {
        /// <inheritdoc />
        public override byte[] MessageTypes { get; } = { MessageContextConstants.MessageTypeEvent };

        /// <summary>
        /// Options.
        /// </summary>
        public new EventPipelineOptions Options { get; } = new EventPipelineOptions();

        /// <inheritdoc />
        public IMessageContext CreateMessageContext(IPipelineService pipelineService, object @event)
        {
            return new MessageContext(pipelineService, @event);
        }
    }
}
