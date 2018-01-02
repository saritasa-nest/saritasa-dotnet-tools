// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Events
{
    /// <summary>
    /// Events pipeline.
    /// </summary>
    public class EventPipeline : MessagePipeline<EventPipelineOptions>, IEventPipeline, IMessageRecordConverter
    {
        /// <inheritdoc />
        public override byte[] MessageTypes { get; } = { MessageContextConstants.MessageTypeEvent };

        #region IEventPipeline

        /// <inheritdoc />
        public IMessageContext CreateMessageContext(IMessagePipelineService pipelineService, object @event)
        {
            var messageContext = new MessageContext(pipelineService, @event);
            messageContext.Pipeline = this;
            return messageContext;
        }

        #endregion

        #region IMessageRecordConverter

        /// <inheritdoc />
        public IMessageContext CreateMessageContext(IMessagePipelineService pipelineService, MessageRecord record)
        {
            var context = new MessageContext(pipelineService)
            {
                ContentId = record.ContentType,
                Content = record.Content,
                Pipeline = this
            };
            return context;
        }

        /// <inheritdoc />
        public MessageRecord CreateMessageRecord(IMessageContext context)
        {
            var record = MessageRecordHelpers.Create(context);
            record.Type = MessageContextConstants.MessageTypeEvent;
            return record;
        }

        #endregion
    }
}
