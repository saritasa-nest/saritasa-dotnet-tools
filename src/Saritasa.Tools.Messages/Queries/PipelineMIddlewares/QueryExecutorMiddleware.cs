// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using System.Threading.Tasks;
    using Abstractions;

    /// <summary>
    /// Executes query delegate.
    /// </summary>
    public class QueryExecutorMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id => "QueryExecutor";

        /// <inheritdoc />
        public virtual void Handle(IMessage message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(QueryMessage)));
            }

            // Invoke method and resolve parameters if needed.
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                queryMessage.Result = queryMessage.Method.Invoke(queryMessage.QueryObject, queryMessage.Parameters);
                queryMessage.Status = ProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                queryMessage.Status = ProcessingStatus.Failed;
                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    queryMessage.Error = innerException;
                    queryMessage.ErrorDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(innerException);
                }
            }
            finally
            {
                stopWatch.Stop();
                queryMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
            }
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(IMessage message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(QueryMessage)));
            }

            // Invoke method and resolve parameters if needed.
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                queryMessage.Result = queryMessage.Method.Invoke(queryMessage.QueryObject, queryMessage.Parameters);
                var taskResult = queryMessage.Result as Task;
                if (taskResult != null)
                {
                    await taskResult;
                }
                queryMessage.Status = ProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                queryMessage.Status = ProcessingStatus.Failed;
                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    queryMessage.Error = innerException;
                    queryMessage.ErrorDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(innerException);
                }
            }
            finally
            {
                stopWatch.Stop();
                queryMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
            }
        }
    }
}
