// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Common.PipelineMiddlewares;

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
        public QueryPipelineBuilder Configure()
        {
            return Configure(options => { });
        }

        /// <summary>
        /// Use default middlewares configuration. Includes query handler resolver and executor.
        /// </summary>
        /// <returns>Query pipeline builder.</returns>
        public QueryPipelineBuilder Configure(Action<QueryPipelineOptions> optionsAction)
        {
            var options = new QueryPipelineOptions();
            optionsAction(options);

            if (options.UseDefaultPipeline)
            {
                Pipeline.AddMiddlewares(new PipelineMiddlewares.QueryObjectResolverMiddleware());
                Pipeline.AddMiddlewares(new PipelineMiddlewares.QueryExecutorMiddleware
                {
                    IncludeExecutionDuration = options.IncludeExecutionDuration
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
