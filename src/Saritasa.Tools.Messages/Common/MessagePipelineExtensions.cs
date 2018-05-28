// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Message pipeline extensions.
    /// </summary>
    public static class MessagePipelineExtensions
    {
        /// <summary>
        /// Add middlewares to pipeline.
        /// </summary>
        /// <param name="pipeline">Pipeline to add middlewares.</param>
        /// <param name="middlewares">Middlewares to add.</param>
        public static void AddMiddlewares(this IMessagePipeline pipeline,
            params IMessagePipelineMiddleware[] middlewares)
        {
            if (middlewares == null)
            {
                throw new ArgumentNullException(nameof(middlewares));
            }
            if (pipeline.Middlewares == null)
            {
                pipeline.Middlewares = middlewares;
                return;
            }
            var list = pipeline.Middlewares.ToList();
            list.AddRange(middlewares);
            pipeline.Middlewares = list.ToArray();
        }

        /// <summary>
        /// Returns middleware in pipeline with specified id or generates <see cref="InvalidOperationException" />.
        /// </summary>
        /// <param name="pipeline">Pipeline to search.</param>
        /// <param name="id">Middleware id.</param>
        /// <returns>Found middleware.</returns>
        public static IMessagePipelineMiddleware GetMiddlewareById(this IMessagePipeline pipeline, string id)
        {
            var middleware = pipeline.Middlewares.FirstOrDefault(m => m.Id == id);
            if (middleware == null)
            {
                throw new InvalidOperationException($"Cannot find middleware with id {id} in pipeline {pipeline}.");
            }
            return middleware;
        }

        /// <summary>
        /// Replaces middleware with specific id with another one.
        /// </summary>
        /// <param name="pipeline">Pipeline to search.</param>
        /// <param name="id">Middleware id to replace by.</param>
        /// <param name="targetMiddleware">Target middleware.</param>
        public static void ReplaceMiddlewareById(this IMessagePipeline pipeline, string id,
            IMessagePipelineMiddleware targetMiddleware)
        {
            var middleware = GetMiddlewareById(pipeline, id);
            var middlewaresList = pipeline.Middlewares.ToList();
            middlewaresList[middlewaresList.IndexOf(middleware)] = targetMiddleware;
            pipeline.Middlewares = middlewaresList.ToArray();
        }

        /// <summary>
        /// Removes middleware with specific id.
        /// </summary>
        /// <param name="pipeline">Pipeline to search.</param>
        /// <param name="id">Middleware id to remove.</param>
        public static void RemoveMiddlewareById(this IMessagePipeline pipeline, string id)
        {
            var middleware = GetMiddlewareById(pipeline, id);
            var middlewaresList = pipeline.Middlewares.ToList();
            middlewaresList.Remove(middleware);
            pipeline.Middlewares = middlewaresList.ToArray();
        }
    }
}
