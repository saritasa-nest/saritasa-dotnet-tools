// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;

namespace Saritasa.Tools.Messages.Events
{
    /// <summary>
    /// Events pipeline extensions.
    /// </summary>
    public static class MessagePipelinesContainerExtensions
    {
        /// <summary>
        /// Adds event pipeline feature to message context.
        /// </summary>
        /// <param name="pipelineContainer">Pipelines container.</param>
        /// <returns>Event pipeline builder.</returns>
        public static EventPipelineBuilder AddEventPipeline(this IMessagePipelineContainer pipelineContainer)
        {
            if (pipelineContainer.Pipelines.Any(p => p is IEventPipeline))
            {
                throw new InvalidOperationException(
                    "Events pipeline already exists in global context items. " +
                           "Use RemovePipeline method to clean up existing pipeline.");
            }

            var eventPipeline = new EventPipeline();
            var list = pipelineContainer.Pipelines.ToList();
            list.Add(eventPipeline);
            pipelineContainer.Pipelines = list.ToArray();

            return new EventPipelineBuilder(eventPipeline);
        }
    }
}
