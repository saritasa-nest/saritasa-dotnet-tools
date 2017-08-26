// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Events
{
    /// <summary>
    /// Event pipeline builder.
    /// </summary>
    public class EventPipelineBuilder : BasePipelineBuilder<IEventPipeline>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="pipeline">Event pipeline.</param>
        public EventPipelineBuilder(IEventPipeline pipeline) : base(pipeline)
        {
        }

        /// <summary>
        /// Add middleware to events pipeline.
        /// </summary>
        /// <param name="middleware">Middleware to add.</param>
        /// <returns>Command pipeline builder.</returns>
        public EventPipelineBuilder AddMiddleware(IMessagePipelineMiddleware middleware)
        {
            Pipeline.AddMiddlewares(middleware);
            return this;
        }

        /// <summary>
        /// Use default middlewares configuration. Includes event handler locator and executor.
        /// </summary>
        /// <param name="assemblies">Assemblies to search event handlers.</param>
        /// <returns>Event pipeline builder.</returns>
        public EventPipelineBuilder UseDefaultMiddlewares(params Assembly[] assemblies)
        {
            Pipeline.AddMiddlewares(new PipelineMiddlewares.EventHandlerLocatorMiddleware(assemblies));
            Pipeline.AddMiddlewares(new PipelineMiddlewares.EventHandlerResolverMiddleware());
            Pipeline.AddMiddlewares(new PipelineMiddlewares.EventHandlerExecutorMiddleware());
            return this;
        }
    }
}
