// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// Query pipeline extensions.
    /// </summary>
    public static class QueryPipelineExtensions
    {
        /// <summary>
        /// Add query pipeline feature to message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <returns>Query pipeline builder.</returns>
        public static QueryPipelineBuilder AddQueryPipeline(this IPipelinesService pipelinesService)
        {
            return AddQueryPipeline(pipelinesService, options => { });
        }

        /// <summary>
        /// Add query pipeline feature to message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="setupAction">Action to setup query pipeline.</param>
        /// <returns>Query pipeline builder.</returns>
        public static QueryPipelineBuilder AddQueryPipeline(this IPipelinesService pipelinesService,
            Action<QueryPipelineOptions> setupAction)
        {
            if (pipelinesService.Pipelines.Any(p => p is IEventPipeline))
            {
                throw new InvalidOperationException("Queries pipeline already exists in global context items. " +
                                                    "Use RemovePipeline method to clean up existins pipeline.");
            }

            var queryPipeline = new QueryPipeline();
            setupAction(queryPipeline.Options);
            var list = pipelinesService.Pipelines.ToList();
            list.Add(queryPipeline);
            pipelinesService.Pipelines = list.ToArray();

            return new QueryPipelineBuilder(queryPipeline);
        }
    }
}
