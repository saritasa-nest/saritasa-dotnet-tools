// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Common;
    using Common.Expressions;

    /// <summary>
    /// Executes query delegate.
    /// </summary>
    public class QueryExecutorMiddleware : IMessagePipelineMiddleware
    {
        private readonly ExpressionExecutorFactory expressionExecutorFactory;

        /// <summary>
        /// Ctor.
        /// </summary>
        public QueryExecutorMiddleware()
        {
            expressionExecutorFactory = new ExpressionExecutorFactory(ExpressionExecutorServices.Instance);
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

                dynamic result;
                if (!TryInvokeExpression(queryMessage.Method, queryMessage.QueryObject, queryMessage.Parameters, out result))
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

        private bool TryInvokeExpression(MethodInfo info, dynamic input, dynamic[] parameters, out dynamic result)
        {
            result = null;

            var executor = expressionExecutorFactory.Create();
            if (!executor.CompiledCache.HasKey(info))
            {
                return false;
            }

            var @params = new dynamic[parameters.Length + 1];
            @params[0] = input;

            for (int paramsIndex = 1, parametersIndex = 0; parametersIndex < parameters.Length; paramsIndex++, parametersIndex++)
            {
                @params[paramsIndex] = parameters[parametersIndex];
            }

            result = executor.Execute(info, @params);

            return true;
        }
    }
}
