// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Provides common functionality for Command/Query/Event executor middlewares.
    /// </summary>
    public abstract class BaseHandlerExecutorMiddleware : IMessagePipelinePostAction
    {
        /// <summary>
        /// If <c>true</c> the middleware will try to resolve executing method parameters. Default is <c>true</c>.
        /// </summary>
        public bool UseParametersResolve { get; set; } = true;

        /// <summary>
        /// Captures <see cref="System.Runtime.ExceptionServices.ExceptionDispatchInfo" /> of original execution exception
        /// as item with ".exception-dispatch" key. Default is <c>false</c>.
        /// </summary>
        public bool CaptureExceptionDispatchInfo { get; set; } = false;

        private readonly bool throwExceptionsOnFail;

        private delegate object HandlerCall(object handler, object obj, IServiceProvider serviceProvider,
            CancellationToken cancellationToken);

        private readonly ConcurrentDictionary<MethodInfo, HandlerCall> methodFuncCache =
            new ConcurrentDictionary<MethodInfo, HandlerCall>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="throwExceptionsOnFail">If there were exception during processing it will be rethrown. Default is <c>true</c>.</param>
        protected BaseHandlerExecutorMiddleware(bool throwExceptionsOnFail = true)
        {
            this.throwExceptionsOnFail = throwExceptionsOnFail;
        }

        /// <summary>
        /// Executes method. If method is awaitable it will wait for it.
        /// </summary>
        /// <param name="handler">Handler.</param>
        /// <param name="obj">The first argument.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="handlerMethod">Method to execute.</param>
        protected void ExecuteHandler(object handler, object obj, IServiceProvider serviceProvider,
            MethodInfo handlerMethod)
        {
            var func = CreateInvokeHandlerMethodExpressionWithCache(handler, obj, handlerMethod);
            var task = func(handler, obj, serviceProvider, default(CancellationToken)) as Task;
            task?.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes method in async mode. Handler should return <see cref="Task" /> otherwise
        /// it will be executed in sync mode.
        /// </summary>
        /// <param name="handler">Async handler.</param>
        /// <param name="obj">The first argument.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="handlerMethod">Method to execute.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        protected async Task ExecuteHandlerAsync(object handler, object obj, IServiceProvider serviceProvider,
            MethodInfo handlerMethod, CancellationToken cancellationToken = default(CancellationToken))
        {
            var func = CreateInvokeHandlerMethodExpressionWithCache(handler, obj, handlerMethod);
            var task = func(handler, obj, serviceProvider, cancellationToken) as Task;
            if (task != null)
            {
                await task;
            }
        }

        private HandlerCall CreateInvokeHandlerMethodExpressionWithCache(
            object handler, object obj, MethodInfo handlerMethod)
        {
            return methodFuncCache.GetOrAdd(
                handlerMethod,
                mi => CreateInvokeHandlerMethodExpression(handler, obj, handlerMethod).Compile());
        }

        /// <summary>
        /// Constructs the expression like this:
        /// handler.Method(obj, (IInterface)sp.Resolve(typeof(IInterface))) .
        /// </summary>
        /// <param name="handler">Handler object.</param>
        /// <param name="obj">Command object.</param>
        /// <param name="handlerMethod">Handler method to call.</param>
        /// <returns>Expression.</returns>
        private Expression<HandlerCall> CreateInvokeHandlerMethodExpression(
            object handler, object obj, MethodInfo handlerMethod)
        {
            var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "sp");
            var objectParam = Expression.Parameter(typeof(object), "obj");
            var handlerParam = Expression.Parameter(typeof(object), "handler");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            var getServiceMethod = typeof(IServiceProvider).GetTypeInfo().GetMethod("GetService");
            var paramsExpressions = new List<Expression>();
            var expressions = new List<Expression>();

            // Prepare parameters for function call.
            var handlerMethodParameters = handlerMethod.GetParameters();
            if (UseParametersResolve)
            {
                if (handlerMethodParameters.Length > 1)
                {
                    if (handlerMethod.DeclaringType != obj.GetType())
                    {
                        paramsExpressions.Add(Expression.Convert(objectParam, obj.GetType()));
                        for (int i = 1; i < handlerMethodParameters.Length; i++)
                        {
                            paramsExpressions.Add(Expression.Convert(
                                Expression.Call(serviceProviderParam, getServiceMethod,
                                    Expression.Constant(handlerMethodParameters[i].ParameterType)),
                                handlerMethodParameters[i].ParameterType));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < handlerMethodParameters.Length; i++)
                        {
                            paramsExpressions.Add(Expression.Convert(
                                Expression.Call(serviceProviderParam, getServiceMethod,
                                    Expression.Constant(handlerMethodParameters[i].ParameterType)),
                                handlerMethodParameters[i].ParameterType));
                        }
                    }
                }
                if (handlerMethodParameters.Length == 1)
                {
                    paramsExpressions.Add(Expression.Convert(objectParam, obj.GetType()));
                }
            }
            else if (handlerMethodParameters.Length > 0)
            {
                paramsExpressions.Add(Expression.Convert(objectParam, obj.GetType()));
            }

            // If last parameter is cancellation token - we pass it.
            if (handlerMethodParameters.Length > 1 &&
                handlerMethodParameters[handlerMethodParameters.Length - 1].ParameterType == typeof(CancellationToken))
            {
                paramsExpressions.RemoveAt(paramsExpressions.Count - 1);
                paramsExpressions.Add(cancellationTokenParam);
            }

            // Call function with parameters.
            var callExpression = Expression.Call(
                Expression.Convert(handlerParam, handler.GetType()),
                handlerMethod,
                paramsExpressions);

            expressions.Add(callExpression);
            // If function does not have return we put null instead.
            if (handlerMethod.ReturnType == typeof(void))
            {
                expressions.Add(Expression.Constant(null, typeof(object)));
            }

            return Expression.Lambda<HandlerCall>(
                Expression.Block(expressions), handlerParam, objectParam, serviceProviderParam, cancellationTokenParam);
        }

        /// <inheritdoc />
        public void PostHandle(IMessageContext messageContext)
        {
            if (throwExceptionsOnFail)
            {
                InternalThrowProcessingException(messageContext, CaptureExceptionDispatchInfo);
            }
        }

        internal static void InternalThrowProcessingException(IMessageContext messageContext, bool captureExceptionDispatchInfo)
        {
            if (captureExceptionDispatchInfo)
            {
                var edi = messageContext.GetItemByKeyOrDefault<System.Runtime.ExceptionServices.ExceptionDispatchInfo>(MessageContextConstants.ExceptionDispatchInfoKey);
                edi?.Throw();
            }
            if (messageContext.FailException != null)
            {
                throw new MessageProcessingException("Processing exception.", messageContext.FailException);
            }
        }
    }
}
