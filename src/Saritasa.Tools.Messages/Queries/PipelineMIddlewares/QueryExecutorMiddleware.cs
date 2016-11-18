// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Common;
    using Saritasa.Tools.Messages.Common.Expressions;

    /// <summary>
    /// Executes query delegate.
    /// </summary>
    public class QueryExecutorMiddleware : IMessagePipelineMiddleware
    {
        private readonly ExpressionExecutorFactory expressionExecutorFactory;

        public QueryExecutorMiddleware()
        {
            this.expressionExecutorFactory = new ExpressionExecutorFactory(new ExpressionExecutorServices());
        }

        /// <inheritdoc />
        public string Id => "QueryExecutor";

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new ArgumentException("Message should be QueryMessage type");
            }

            // invoke method and resolve parameters if needed
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                queryMessage.Result = InvokeExpression(queryMessage.Method, queryMessage.QueryObject, queryMessage.Parameters.Length, queryMessage.Parameters);
                // queryMessage.Result = queryMessage.Method.Invoke(queryMessage.QueryObject, queryMessage.Parameters);
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

        private dynamic InvokeExpression(MethodInfo info, dynamic input, int parametersCount, dynamic[] parameters)
        {
            var executor = expressionExecutorFactory.Create();

            if (parametersCount == 0)
            {
                return executor.Execute(info, input);
            }
            else if (parametersCount == 1)
            {
                return executor.Execute(info, input, parameters[0]);
            }
            else if (parametersCount == 2)
            {
                return executor.Execute(info, input, parameters[0], parameters[1]);
            }
            else if (parametersCount == 3)
            {
                return executor.Execute(info, input, parameters[0], parameters[1], parameters[2]);
            }
            else if (parametersCount == 4)
            {
                return executor.Execute(info, input, parameters[0], parameters[1], parameters[2], parameters[3]);
            }

            throw new NotSupportedException();
        }
    }
}
