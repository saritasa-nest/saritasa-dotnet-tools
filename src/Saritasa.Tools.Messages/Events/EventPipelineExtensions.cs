// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions.Events;

namespace Saritasa.Tools.Messages.Events
{
    /// <summary>
    /// Event pipeline extensions.
    /// </summary>
    public static class EventPipelineExtensions
    {
        /// <summary>
        /// Add events pipeline feature to message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <returns>Event pipeline builder.</returns>
        public static EventPipelineBuilder AddEventPipeline(this IPipelinesService pipelinesService)
        {
            return AddEventPipeline(pipelinesService, options => { });
        }

        /// <summary>
        /// Add event pipeline feature to message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="setupAction">Action to setup event pipeline.</param>
        /// <returns>Event pipeline builder.</returns>
        public static EventPipelineBuilder AddEventPipeline(this IPipelinesService pipelinesService,
            Action<EventPipelineOptions> setupAction)
        {
            if (pipelinesService.Pipelines.Any(p => p is IEventPipeline))
            {
                throw new InvalidOperationException("Events pipeline already exists in global context items. " +
                                                    "Use RemovePipeline method to clean up existins pipeline.");
            }

            var eventPipeline = new EventPipeline();
            setupAction(eventPipeline.Options);
            var list = pipelinesService.Pipelines.ToList();
            list.Add(eventPipeline);
            pipelinesService.Pipelines = list.ToArray();

            return new EventPipelineBuilder(eventPipeline);
        }
    }
}
