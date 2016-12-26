// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Events
{
    using Common;
    using Abstractions;

    /// <summary>
    /// Event pipeline extensions.
    /// </summary>
    public static class EventPipelineExtensions
    {
        /// <summary>
        /// Use internal IoC container.
        /// </summary>
        /// <param name="eventPipeline">Event pipeline.</param>
        /// <param name="resolveMethodParameters">Resolve method parameters.</param>
        /// <returns>Event pipeline.</returns>
        public static IEventPipeline UseInternalResolver(this IEventPipeline eventPipeline,
            bool resolveMethodParameters = true)
        {
            var middleware = (PipelineMiddlewares.EventExecutorMiddleware)eventPipeline.GetMiddlewareById("EventExecutor");
            if (middleware == null)
            {
                throw new MiddlewareNotFoundException();
            }
            middleware.UseInternalObjectResolver = true;
            middleware.UseParametersResolve = resolveMethodParameters;
            return eventPipeline;
        }

        /// <summary>
        /// Use another method to search handlers.
        /// </summary>
        /// <param name="eventPipeline">Event pipeline.</param>
        /// <param name="searchMethod">Handlers search method.</param>
        /// <returns>Event pipeline.</returns>
        public static IEventPipeline UseHandlerSearchMethod(this IEventPipeline eventPipeline,
            HandlerSearchMethod searchMethod)
        {
            var middleware = (PipelineMiddlewares.EventHandlerLocatorMiddleware)eventPipeline.GetMiddlewareById("EventLocator");
            if (middleware == null)
            {
                throw new MiddlewareNotFoundException();
            }
            middleware.HandlerSearchMethod = searchMethod;
            return eventPipeline;
        }
    }
}
