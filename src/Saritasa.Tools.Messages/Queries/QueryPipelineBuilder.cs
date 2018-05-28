// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// Query pipeline builder.
    /// </summary>
    public class QueryPipelineBuilder : BasePipelineBuilder<IQueryPipeline>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="pipeline">Query pipeline.</param>
        public QueryPipelineBuilder(IQueryPipeline pipeline) : base(pipeline)
        {
        }

        /// <summary>
        /// Add middleware to events pipeline.
        /// </summary>
        /// <param name="middleware">Middleware to add.</param>
        /// <returns>Command pipeline builder.</returns>
        public QueryPipelineBuilder AddMiddleware(IMessagePipelineMiddleware middleware)
        {
            Pipeline.AddMiddlewares(middleware);
            return this;
        }

        /// <summary>
        /// Use default middlewares configuration. Includes query handler resolver and executor.
        /// </summary>
        public QueryPipelineBuilder AddStandardMiddlewares()
        {
            return AddStandardMiddlewares(options => { });
        }

        /// <summary>
        /// Use default middlewares configuration. Includes query handler resolver and executor.
        /// </summary>
        /// <returns>Query pipeline builder.</returns>
        public QueryPipelineBuilder AddStandardMiddlewares(Action<QueryPipelineOptions> optionsAction)
        {
            var options = new QueryPipelineOptions();
            optionsAction(options);

            if (options.InternalResolver.HasAssemblies && !options.InternalResolver.UseInternalObjectResolver)
            {
                throw new InvalidOperationException(
                    "Assemblies to search handlers were provided but internal object resolver is not used.");
            }
            if (options.InternalResolver.UseHandlerParametersResolve)
            {
                throw new InvalidOperationException("Handler method parameters resolve does not work for query pipeline.");
            }

            // If assemblies were provided that means we use internal object resolver.
            if (options.InternalResolver.HasAssemblies)
            {
                Pipeline.AddMiddlewares(new PipelineMiddlewares.QueryObjectResolverMiddleware(
                    options.InternalResolver.Assemblies.ToArray()));
            }
            else
            {
                Pipeline.AddMiddlewares(new PipelineMiddlewares.QueryObjectResolverMiddleware(
                    options.InternalResolver.UseInternalObjectResolver));
            }
            Pipeline.AddMiddlewares(new PipelineMiddlewares.QueryExecutorMiddleware(options.ThrowExceptionOnFail)
            {
                IncludeExecutionDuration = options.IncludeExecutionDuration,
                CaptureExceptionDispatchInfo = options.UseExceptionDispatchInfo
            });
            return this;
        }

        /// <summary>
        /// Returns built pipeline.
        /// </summary>
        /// <returns>Query pipeline.</returns>
        public IQueryPipeline Get() => Pipeline;
    }
}
