// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;
using System.Collections.Generic;
using System.Threading;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Provides common functionality for Command/Query/Event executor middlewares.
    /// </summary>
    public abstract class BaseExecutorMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        private const string KeyId = "id";
        private const string KeyMethod = "method";
        private const string KeyClass = "class";

        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// If true the middleware will resolve project using internal resolver.
        /// </summary>
        public bool UseInternalObjectResolver { get; set; }

        /// <summary>
        /// If true the middleware will try to resolve executing method parameters. Default is false.
        /// </summary>
        public bool UseParametersResolve { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        protected BaseExecutorMiddleware(IDictionary<string, string> parameters) : this()
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.ContainsKey(KeyId))
            {
                Id = parameters[KeyId];
            }
        }

        /// <summary>
        /// .ctor
        /// </summary>
        protected BaseExecutorMiddleware()
        {
            Id = this.GetType().Name;
        }

        /// <inheritdoc />
        public abstract void Handle(IMessageContext messageContext);

        /// <inheritdoc />
        public abstract Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken);

        /// <summary>
        /// If UseInternalObjectResolver is turned off internal IoC container is used. Otherwise
        /// it relies on provided IoC implementation.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="loggingSource">Logging source. Optional.</param>
        /// <returns>Object or null if cannot be resolved.</returns>
        protected object ResolveObject(Type type, IServiceProvider serviceProvider, string loggingSource = "")
        {
            return UseInternalObjectResolver ?
                TypeHelpers.ResolveObjectForType(type, serviceProvider.GetService, loggingSource) :
                serviceProvider.GetService(type);
        }

        private object[] GetAndResolveHandlerParameters(object obj, IServiceProvider serviceProvider,
            MethodBase handlerMethod)
        {
            if (UseParametersResolve)
            {
                var parameters = handlerMethod.GetParameters();
                var paramsarr = new object[parameters.Length];
                if (parameters.Length > 1)
                {
                    if (handlerMethod.DeclaringType != obj.GetType())
                    {
                        paramsarr[0] = obj;
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            paramsarr[i] = serviceProvider.GetService(parameters[i].ParameterType);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            paramsarr[i] = serviceProvider.GetService(parameters[i].ParameterType);
                        }
                    }
                    return paramsarr;
                }
                if (parameters.Length == 1)
                {
                    paramsarr[0] = obj;
                }
                return paramsarr;
            }
            return handlerMethod.GetParameters().Length > 0 ? new[] { obj } : new object[] { };
        }

        /// <summary>
        /// Execute method. If method is awaitable method will wait for it.
        /// </summary>
        /// <param name="handler">Handler.</param>
        /// <param name="obj">The first argument.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="handlerMethod">Method to execute.</param>
        protected void ExecuteHandler(object handler, object obj, IServiceProvider serviceProvider, MethodBase handlerMethod)
        {
            var result = handlerMethod.Invoke(handler, GetAndResolveHandlerParameters(obj, serviceProvider, handlerMethod));
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
            MethodBase handlerMethod)
        {
            var task = handlerMethod.Invoke(handler, GetAndResolveHandlerParameters(obj, serviceProvider, handlerMethod)) as Task;
            if (task != null)
            {
                await task;
            }
        }
    }
}
