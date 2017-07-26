// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions.Queries;

// ReSharper disable once CheckNamespace
namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Query pipelines extensions.
    /// </summary>
    public static class QueryPipelinesServiceExtensions
    {
        /// <summary>
        /// Query.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <returns>Query caller.</returns>
        public static IQueryCaller<TQuery> Query<TQuery>(this IMessagePipelineService pipelineService) where TQuery : class
        {
            var pipeline = pipelineService.GetPipelineOfType<IQueryPipeline>();
            return pipeline.Query<TQuery>(pipelineService);
        }

        /// <summary>
        /// Query.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <param name="obj">Query object to execute by.</param>
        /// <returns>Query caller.</returns>
        public static IQueryCaller<TQuery> Query<TQuery>(this IMessagePipelineService pipelineService,
            TQuery obj) where TQuery : class
        {
            var pipeline = pipelineService.GetPipelineOfType<IQueryPipeline>();
            return pipeline.Query<TQuery>(pipelineService, obj);
        }
    }
}
