// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

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
        public static IEventPipeline UseInternalResolver(this IEventPipeline eventPipeline,
            bool resolveMethodParameters = false)
        {
            var middleware = (PipelineMiddlewares.EventExecutorMiddleware)eventPipeline.GetMiddlewareById("EventExecutor");
            middleware.UseInternalObjectResolver = true;
            middleware.UseParametersResolve = resolveMethodParameters;
            return eventPipeline;
        }
    }
}
