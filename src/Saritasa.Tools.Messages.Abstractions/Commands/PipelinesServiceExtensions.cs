// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Abstractions.Commands
{
    /// <summary>
    /// Command pipeline extensions.
    /// </summary>
    public static class PipelinesServiceExtensions
    {
        /// <summary>
        /// Handle command within message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="command">Command to execute.</param>
        /// <returns>Message context used in execution.</returns>
        public static IMessageContext HandleCommand(this IPipelinesService pipelinesService, object command)
        {
            var commandPipeline = pipelinesService.GetPipelineOfType<ICommandPipeline>();
            var messageContext = commandPipeline.CreateMessageContext(pipelinesService, command);
            commandPipeline.Invoke(messageContext);
            return messageContext;
        }

        /// <summary>
        /// Handle command within message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Message context used in execution.</returns>
        public static async Task<IMessageContext> HandleCommandAsync(this IPipelinesService pipelinesService,
            object command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandPipeline = pipelinesService.GetPipelineOfType<ICommandPipeline>();
            var messageContext = commandPipeline.CreateMessageContext(pipelinesService, command);
            await commandPipeline.InvokeAsync(messageContext, cancellationToken);
            return messageContext;
        }
    }
}
