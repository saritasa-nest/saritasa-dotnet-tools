// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    /// <summary>
    /// Executes query delegate.
    /// </summary>
    public class QueryExecutorMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        /// <summary>
        /// Middleware identifier.
        /// </summary>
        public string Id { get; set; } = nameof(QueryExecutorMiddleware);

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            var queryParams = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);

            // Invoke method and resolve parameters if needed.
            Stopwatch stopwatch = null;
            var queryPipeline = messageContext.Pipeline as QueryPipeline;
            if (queryPipeline != null && queryPipeline.Options.IncludeExecutionDuration)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            try
            {
                queryParams.Result = queryParams.Method.Invoke(queryParams.QueryObject, queryParams.Parameters);
                messageContext.Items[MessageContextConstants.ResultKey] = queryParams.Result;
                messageContext.Status = ProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                messageContext.Status = ProcessingStatus.Failed;
                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    messageContext.FailException = innerException;
                }
            }
            finally
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    messageContext.Items[MessageContextConstants.ExecutionDurationKey] = (int)stopwatch.ElapsedMilliseconds;
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var queryParams = (QueryParameters)messageContext.Items[QueryPipeline.QueryParametersKey];

            // Invoke method and resolve parameters if needed.
            Stopwatch stopwatch = null;
            var queryPipeline = messageContext.Pipeline as QueryPipeline;
            if (queryPipeline != null && queryPipeline.Options.IncludeExecutionDuration)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            try
            {
                queryParams.Result = queryParams.Method.Invoke(queryParams.QueryObject, queryParams.Parameters);
                var taskResult = queryParams.Result as Task;
                if (taskResult != null)
                {
                    await taskResult;
                }
                messageContext.Status = ProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                messageContext.Status = ProcessingStatus.Failed;
                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    messageContext.FailException = innerException;
                }
            }
            finally
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    messageContext.Items[MessageContextConstants.ExecutionDurationKey] = (int)stopwatch.ElapsedMilliseconds;
                }
            }
        }
    }
}
