// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
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
        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public QueryExecutorMiddleware()
        {
            Id = this.GetType().Name;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            var queryParams = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);

            // Invoke method and resolve parameters if needed.
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
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var queryParams = (QueryParameters)messageContext.Items[QueryPipeline.QueryParametersKey];

            // Invoke method and resolve parameters if needed.
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
        }
    }
}
