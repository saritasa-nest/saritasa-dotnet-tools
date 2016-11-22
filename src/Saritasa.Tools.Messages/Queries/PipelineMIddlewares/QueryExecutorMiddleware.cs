// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                if (!TryInvokeExpression(queryMessage.Method, queryMessage.QueryObject, queryMessage.Parameters.Length, queryMessage.Parameters, out var result))
                {
                    result = queryMessage.Method.Invoke(queryMessage.QueryObject, queryMessage.Parameters);
                }

                queryMessage.Result = result;
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

        private bool TryInvokeExpression(MethodInfo info, dynamic input, int parametersCount, dynamic[] parameters, out dynamic result)
        {
            result = null;
            try
            {
                var executor = expressionExecutorFactory.Create();
                var @params = new[] { input }.Concat(parameters).ToArray();

                result = executor.Execute(info, @params);

                return true;
            }
            catch (Exception)
            {
#if !DEBUG
                return false;
#else
                throw;
#endif
            }
        }
    }
}
