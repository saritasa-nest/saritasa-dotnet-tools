// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions.Queries
{
    /// <summary>
    /// Query pipelines extensions.
    /// </summary>
    public static class PipelinesServiceExtensions
    {
        /// <summary>
        /// Query.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <returns>Query caller.</returns>
        public static IQueryCaller<TQuery> Query<TQuery>(this IPipelinesService pipelinesService) where TQuery : class
        {
            var pipeline = pipelinesService.GetPipelineOfType<IQueryPipeline>();
            return pipeline.Query<TQuery>(pipelinesService);
        }

        /// <summary>
        /// Query.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="obj">Query object to execute by.</param>
        /// <returns>Query caller.</returns>
        public static IQueryCaller<TQuery> Query<TQuery>(this IPipelinesService pipelinesService,
            TQuery obj) where TQuery : class
        {
            var pipeline = pipelinesService.GetPipelineOfType<IQueryPipeline>();
            return pipeline.Query<TQuery>(pipelinesService, obj);
        }
    }
}
