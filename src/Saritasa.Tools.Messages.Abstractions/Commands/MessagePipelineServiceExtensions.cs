// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions.Commands;

// ReSharper disable once CheckNamespace
namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Command pipeline extensions.
    /// </summary>
    public static partial class MessagePipelineServiceExtensions
    {
        /// <summary>
        /// Handle command within message context.
        /// </summary>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <param name="command">Command to execute.</param>
        /// <returns>Message context used in execution.</returns>
        public static IMessageContext HandleCommand(this IMessagePipelineService pipelineService, object command)
        {
            var pipeline = pipelineService.GetPipelineOfType<ICommandPipeline>();
            var messageContext = pipeline.CreateMessageContext(pipelineService, command);
            pipeline.Invoke(messageContext);
            return messageContext;
        }

        /// <summary>
        /// Handle command within message context with additional parameter.
        /// </summary>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="param">Additional parameter to provide for middleware.</param>
        /// <returns>Message context used in execution.</returns>
        public static IMessageContext HandleCommand(this IMessagePipelineService pipelineService, object command,
            object param)
        {
            var pipeline = pipelineService.GetPipelineOfType<ICommandPipeline>();
            var messageContext = pipeline.CreateMessageContext(pipelineService, command);
            if (param != null)
            {
                messageContext.Items[MessageContextConstants.ParamKey] = param;
            }
            pipeline.Invoke(messageContext);
            return messageContext;
        }

        /// <summary>
        /// Handle command within message context with additional parameter.
        /// </summary>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="param">Additional parameter to provide for middleware.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Message context used in execution.</returns>
        public static async Task<IMessageContext> HandleCommandAsync(this IMessagePipelineService pipelineService,
            object command, object param, CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandPipeline = pipelineService.GetPipelineOfType<ICommandPipeline>();
            var messageContext = commandPipeline.CreateMessageContext(pipelineService, command);
            if (param != null)
            {
                messageContext.Items[MessageContextConstants.ParamKey] = param;
            }
            await commandPipeline.InvokeAsync(messageContext, cancellationToken);
            return messageContext;
        }

        /// <summary>
        /// Handle command within message context.
        /// </summary>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Message context used in execution.</returns>
        public static async Task<IMessageContext> HandleCommandAsync(this IMessagePipelineService pipelineService,
            object command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandPipeline = pipelineService.GetPipelineOfType<ICommandPipeline>();
            var messageContext = commandPipeline.CreateMessageContext(pipelineService, command);
            await commandPipeline.InvokeAsync(messageContext, cancellationToken);
            return messageContext;
        }
    }
}
