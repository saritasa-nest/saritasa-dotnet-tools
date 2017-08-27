// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Provides common functionality for Command/Query/Event executor middlewares.
    /// </summary>
    public abstract class BaseHandlerExecutorMiddleware
    {
        /// <summary>
        /// If true the middleware will try to resolve executing method parameters. Default is <c>true</c>.
        /// </summary>
        public bool UseParametersResolve { get; set; } = true;

        private delegate object HandlerCall(object handler, object obj, IServiceProvider serviceProvider);

        private readonly ConcurrentDictionary<MethodInfo, HandlerCall> methodFuncCache =
            new ConcurrentDictionary<MethodInfo, HandlerCall>();

        /// <summary>
        /// Execute method. If method is awaitable method will wait for it.
        /// </summary>
        /// <param name="handler">Handler.</param>
        /// <param name="obj">The first argument.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="handlerMethod">Method to execute.</param>
        protected void ExecuteHandler(object handler, object obj, IServiceProvider serviceProvider,
            MethodInfo handlerMethod)
        {
            var func = CreateInvokeHandlerMethodExpressionWithCache(handler, obj, handlerMethod);
            var result = func(handler, obj, serviceProvider);
            var task = result as Task;
            task?.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute method in async mode. Handler should return <see cref="Task" /> otherwise
        /// it will be executed in sync mode.
        /// </summary>
        /// <param name="handler">Async handler.</param>
        /// <param name="obj">The first argument.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="handlerMethod">Method to execute.</param>
        protected async Task ExecuteHandlerAsync(object handler, object obj, IServiceProvider serviceProvider,
            MethodInfo handlerMethod)
        {
            var func = CreateInvokeHandlerMethodExpressionWithCache(handler, obj, handlerMethod);
            var task = func(handler, obj, serviceProvider) as Task;
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
        /// handler.Method(obj, (IInterface)sp.Resolve(typeof(IInterface)));
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
            var getServiceMethod = typeof(IServiceProvider).GetTypeInfo().GetMethod("GetService");
            var paramsExpressions = new List<Expression>();
            var expressions = new List<Expression>();

            // Prepare parameters for function call.
            if (UseParametersResolve)
            {
                var parameters = handlerMethod.GetParameters();
                if (parameters.Length > 1)
                {
                    if (handlerMethod.DeclaringType != obj.GetType())
                    {
                        paramsExpressions.Add(Expression.Convert(objectParam, obj.GetType()));
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            paramsExpressions.Add(Expression.Convert(
                                Expression.Call(serviceProviderParam, getServiceMethod,
                                    Expression.Constant(parameters[i].ParameterType)),
                                parameters[i].ParameterType));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            paramsExpressions.Add(Expression.Convert(
                                Expression.Call(serviceProviderParam, getServiceMethod,
                                    Expression.Constant(parameters[i].ParameterType)),
                                parameters[i].ParameterType));
                        }
                    }
                }
                if (parameters.Length == 1)
                {
                    paramsExpressions.Add(Expression.Convert(objectParam, obj.GetType()));
                }
            }
            else if (handlerMethod.GetParameters().Length > 0)
            {
                paramsExpressions.Add(Expression.Convert(objectParam, obj.GetType()));
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
                Expression.Block(expressions), handlerParam, objectParam, serviceProviderParam);
        }
    }
}
