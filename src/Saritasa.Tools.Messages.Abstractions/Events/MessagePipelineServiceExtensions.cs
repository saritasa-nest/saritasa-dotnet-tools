// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions.Events;

// ReSharper disable once CheckNamespace
namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Events pipeline extensions.
    /// </summary>
    public static partial class MessagePipelineServiceExtensions
    {
        /// <summary>
        /// Raises event.
        /// </summary>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="event">Event to process.</param>
        public static void RaiseEvent(this IMessagePipelineService pipelineService, object @event)
        {
            var eventPipeline = pipelineService.GetPipelineOfType<IEventPipeline>();
            var messageContext = eventPipeline.CreateMessageContext(pipelineService, @event);
            eventPipeline.Invoke(messageContext);
        }

        /// <summary>
        /// Raises event with additional parameter.
        /// </summary>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="event">Event to process.</param>
        /// <param name="param">Additional parameter to provide for middleware.</param>
        public static void RaiseEvent(this IMessagePipelineService pipelineService, object @event, object param)
        {
            var eventPipeline = pipelineService.GetPipelineOfType<IEventPipeline>();
            var messageContext = eventPipeline.CreateMessageContext(pipelineService, @event);
            if (param != null)
            {
                messageContext.Items[MessageContextConstants.ParamKey] = param;
            }
            eventPipeline.Invoke(messageContext);
        }

        /// <summary>
        /// Raises async event.
        /// </summary>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="event">Event to process.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public static async Task RaiseEventAsync(this IMessagePipelineService pipelineService, object @event,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventPipeline = pipelineService.GetPipelineOfType<IEventPipeline>();
            var messageContext = eventPipeline.CreateMessageContext(pipelineService, @event);
            await eventPipeline.InvokeAsync(messageContext, cancellationToken);
        }

        /// <summary>
        /// Raises async event with additional parameter.
        /// </summary>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="event">Event to process.</param>
        /// <param name="param">Additional parameter to provide for middleware.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public static async Task RaiseEventAsync(this IMessagePipelineService pipelineService, object @event,
            object param, CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventPipeline = pipelineService.GetPipelineOfType<IEventPipeline>();
            var messageContext = eventPipeline.CreateMessageContext(pipelineService, @event);
            if (param != null)
            {
                messageContext.Items[MessageContextConstants.ParamKey] = param;
            }
            await eventPipeline.InvokeAsync(messageContext, cancellationToken);
        }
    }
}
