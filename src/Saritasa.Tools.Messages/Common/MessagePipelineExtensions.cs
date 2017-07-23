using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
