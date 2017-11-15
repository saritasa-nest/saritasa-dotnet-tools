// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
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
        /// Use default middlewares configuration. Includes command handler resolver, executor and
        /// releaser.
        /// </summary>
        /// <returns>Query pipeline builder.</returns>
        public QueryPipelineBuilder UseDefaultMiddlewares()
        {
            Pipeline.AddMiddlewares(new PipelineMiddlewares.QueryObjectResolverMiddleware());
            Pipeline.AddMiddlewares(new PipelineMiddlewares.QueryExecutorMiddleware());
            return this;
        }
    }
}
