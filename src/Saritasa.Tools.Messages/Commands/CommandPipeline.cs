// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Commands specific pipeline.
    /// </summary>
    public class CommandPipeline : MessagePipeline<CommandPipelineOptions>, ICommandPipeline, IMessageRecordConverter
    {
        /// <inheritdoc />
        public override byte[] MessageTypes { get; } = { MessageContextConstants.MessageTypeCommand };

        #region ICommandPipeline

        /// <inheritdoc />
        public IMessageContext CreateMessageContext(IMessagePipelineService pipelineService, object command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var mc = new MessageContext(pipelineService)
            {
                Content = command,
                ContentId = TypeHelpers.GetPartiallyAssemblyQualifiedName(command.GetType()),
                Pipeline = this
            };
            return mc;
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
            record.Type = MessageContextConstants.MessageTypeCommand;
            return record;
        }

        #endregion
    }
}
