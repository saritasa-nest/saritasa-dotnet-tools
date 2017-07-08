// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Events
{
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
        public static IEventPipeline UseInternalResolver(
            [NotNull] this IEventPipeline eventPipeline,
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
        public static IEventPipeline UseHandlerSearchMethod(
            [NotNull] this IEventPipeline eventPipeline,
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
