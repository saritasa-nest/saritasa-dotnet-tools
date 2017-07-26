// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Abstractions.Queries
{
    /// <summary>
    /// Queries specific pipeline.
    /// </summary>
    public interface IQueryPipeline : IMessagePipeline
    {
        /// <summary>
        /// Creates caller wrapper.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <returns>Query wrapper.</returns>
        IQueryCaller<TQuery> Query<TQuery>([NotNull] IMessagePipelineService pipelineService) where TQuery : class;

        /// <summary>
        /// Creates caller wrapper.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="obj">Query object to execute by.</param>
        /// <returns>Query wrapper.</returns>
        IQueryCaller<TQuery> Query<TQuery>([NotNull] IMessagePipelineService pipelineService,
            [NotNull] TQuery obj) where TQuery : class;

        /// <summary>
        /// Create message context from query call. Does not run query.
        /// </summary>
        /// <typeparam name="TQuery">Query object type.</typeparam>
        /// <param name="pipelineService">Pipeline service.</param>
        /// <param name="messageContext">Message context.</param>
        /// <returns>Default value for TQuery.</returns>
        IQueryCaller<TQuery> CreateMessageContext<TQuery>([NotNull] IMessagePipelineService pipelineService,
            IMessageContext messageContext) where TQuery : class;
    }
}
