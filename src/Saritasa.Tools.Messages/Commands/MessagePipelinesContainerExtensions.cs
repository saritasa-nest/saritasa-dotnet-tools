// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Message pipeline container pipeline extensions for commands.
    /// </summary>
    public static class MessagePipelinesContainerExtensions
    {
        /// <summary>
        /// Adds command pipeline feature to message context.
        /// </summary>
        /// <param name="messagePipelineContainer">Message pipelines container.</param>
        /// <returns>Command pipeline builder.</returns>
        public static CommandPipelineBuilder AddCommandPipeline(this IMessagePipelineContainer messagePipelineContainer)
        {
            if (messagePipelineContainer.Pipelines.Any(p => p is ICommandPipeline))
            {
                throw new InvalidOperationException(
                "Command pipeline already exists in global context items. " +
                       "Use RemovePipeline method to clean up existing pipeline.");
            }

            var commandPipeline = new CommandPipeline();
            var list = messagePipelineContainer.Pipelines.ToList();
            list.Add(commandPipeline);
            messagePipelineContainer.Pipelines = list.ToArray();

            return new CommandPipelineBuilder(commandPipeline);
        }
    }
}
