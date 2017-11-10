// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions.Events
{
    /// <summary>
    /// Events specific pipeline.
    /// </summary>
    public interface IEventPipeline : IMessagePipeline
    {
        /// <summary>
        /// Create message context from pipelines service and event object.
        /// </summary>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="event">Event object.</param>
        /// <returns>Message context.</returns>
        IMessageContext CreateMessageContext(IMessagePipelineService pipelineService, object @event);
    }
}
