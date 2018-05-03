// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    /// <summary>
    /// Executes query delegate.
    /// </summary>
    public class QueryExecutorMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware,
        IMessagePipelinePostAction
    {
        /// <summary>
        /// Middleware identifier.
        /// </summary>
        public string Id { get; set; } = nameof(QueryExecutorMiddleware);

        /// <summary>
        /// Include execution duration.
        /// </summary>
        public bool IncludeExecutionDuration { get; set; } = true;

        /// <summary>
        /// Captures <see cref="ExceptionDispatchInfo" /> of original execution exception
        /// as item with ".exception-dispatch" key. Default is <c>false</c>.
        /// </summary>
        public bool CaptureExceptionDispatchInfo { get; set; } = false;

        private readonly bool throwExceptionOnFail;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="throwExceptionOnFail">If there were exception during processing it will be rethrown. Default is <c>true</c>.</param>
        public QueryExecutorMiddleware(bool throwExceptionOnFail = true)
        {
            this.throwExceptionOnFail = throwExceptionOnFail;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            var queryParams = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);

            // Invoke method and resolve parameters if needed.
            Stopwatch stopwatch = null;
            var queryPipeline = messageContext.Pipeline as QueryPipeline;
            if (queryPipeline != null && IncludeExecutionDuration)
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

            var contentDict = messageContext.Content as IDictionary<string, object>;
            if (contentDict != null)
            {
                contentDict["@result"] = queryParams.Result;
            }
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var queryParams = (QueryParameters)messageContext.Items[QueryPipeline.QueryParametersKey];

            // Invoke method and resolve parameters if needed.
            Stopwatch stopwatch = null;
            var queryPipeline = messageContext.Pipeline as QueryPipeline;
            if (queryPipeline != null && IncludeExecutionDuration)
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
                    if (CaptureExceptionDispatchInfo)
                    {
                        messageContext.Items[MessageContextConstants.ExceptionDispatchInfoKey] =
                            ExceptionDispatchInfo.Capture(messageContext.FailException);
                    }
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
        public void PostHandle(IMessageContext messageContext)
        {
            if (throwExceptionOnFail)
            {
                BaseHandlerExecutorMiddleware.InternalThrowProcessingException(messageContext, CaptureExceptionDispatchInfo);
            }
        }
    }
}
