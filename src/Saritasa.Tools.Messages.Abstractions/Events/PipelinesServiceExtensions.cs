// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Saritasa.Tools.Messages.Abstractions.Events
{
    /// <summary>
    /// Events pipeline extensions.
    /// </summary>
    public static class PipelinesServiceExtensions
    {
        /// <summary>
        /// Raise event.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="event">Event to process.</param>
        public static void RaiseEvent(this IPipelinesService pipelinesService, object @event)
        {
            var eventPipeline = pipelinesService.GetPipelineOfType<IEventPipeline>();
            var messageContext = eventPipeline.CreateMessageContext(pipelinesService, @event);
            eventPipeline.Invoke(messageContext);
        }

        /// <summary>
        /// Raise event.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="event">Event to process.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public static async void RaiseEventAsync(this IPipelinesService pipelinesService, object @event,
            CancellationToken cancellationToken)
        {
            var eventPipeline = pipelinesService.GetPipelineOfType<IEventPipeline>();
            var messageContext = eventPipeline.CreateMessageContext(pipelinesService, @event);
            await eventPipeline.InvokeAsync(messageContext, cancellationToken);
        }
    }
}
