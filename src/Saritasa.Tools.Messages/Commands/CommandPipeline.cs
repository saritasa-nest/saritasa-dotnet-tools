// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Commands specific pipeline.
    /// </summary>
    public class CommandPipeline : MessagePipeline, ICommandPipeline
    {
        /// <inheritdoc />
        public override byte[] MessageTypes { get; } = { MessageContextConstants.MessageTypeCommand };

        /// <summary>
        /// Options.
        /// </summary>
        public new CommandPipelineOptions Options { get; } = new CommandPipelineOptions();

        /// <inheritdoc />
        public IMessageContext CreateMessageContext(IPipelineService pipelineService, object command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var mc = new MessageContext(pipelineService)
            {
                Content = command,
                ContentId = command.GetType().FullName
            };
            return mc;
        }
    }
}
