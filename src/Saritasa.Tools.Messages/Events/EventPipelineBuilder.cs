// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Common.PipelineMiddlewares;

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
        /// <param name="optionsAction">Delegate to configure actions.</param>
        /// <returns>Event pipeline builder.</returns>
        public EventPipelineBuilder Configure(Action<EventPipelineOptions> optionsAction)
        {
            var options = new EventPipelineOptions();
            optionsAction(options);

            if (options.UseDefaultPipeline)
            {
                Pipeline.AddMiddlewares(new PipelineMiddlewares.EventHandlerLocatorMiddleware(
                    options.InternalResolver.GetAssemblies()));
                Pipeline.AddMiddlewares(new PipelineMiddlewares.EventHandlerResolverMiddleware(
                    options.InternalResolver.UseInternalObjectResolver)
                {
                    UsePropertiesResolving = options.InternalResolver.UsePropertiesResolving
                });
                Pipeline.AddMiddlewares(new PipelineMiddlewares.EventHandlerExecutorMiddleware
                {
                    CaptureExceptionDispatchInfo = options.UseExceptionDispatchInfo,
                    UseParametersResolve = options.InternalResolver.UseHandlerParametersResolve
                });
                if (options.ThrowExceptionOnFail)
                {
                    Pipeline.AddMiddlewares(new ThrowExceptionOnFailMiddleware
                    {
                        CheckExceptionDispatchInfo = options.UseExceptionDispatchInfo
                    });
                }
            }

            return this;
        }
    }
}
