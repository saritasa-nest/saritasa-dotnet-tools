// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using Common;

    /// <summary>
    /// Executes query delegate.
    /// </summary>
    public class QueryExecutorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id => "QueryExecutor";

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new NotSupportedException("Message should be QueryMessage type");
            }

            // invoke method and resolve parameters if needed
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                queryMessage.Result = queryMessage.Method.Invoke(queryMessage.QueryObject, queryMessage.Parameters);
                queryMessage.Status = Message.ProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                queryMessage.Status = Message.ProcessingStatus.Failed;
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
