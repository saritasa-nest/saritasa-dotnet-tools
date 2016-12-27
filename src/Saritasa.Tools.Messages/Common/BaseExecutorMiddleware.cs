// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Abstractions;
    using Internal;

    /// <summary>
    /// Provides common functionality for Command/Query/Event executor middlewares.
    /// </summary>
    public abstract class BaseExecutorMiddleware : IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "Executor";

        /// <summary>
        /// Types resolver.
        /// </summary>
        protected Func<Type, object> Resolver { get; set; }

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
        /// <param name="resolver">Types resolver.</param>
        protected BaseExecutorMiddleware(Func<Type, object> resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }
            Resolver = resolver;
        }

        /// <inheritdoc />
        public abstract void Handle(IMessage message);

        /// <inheritdoc />
        public abstract Task HandleAsync(IMessage message);

        /// <summary>
        /// If UseInternalObjectResolver is turned off internal IoC container is used. Otherwise
        /// it relies on provided IoC implementation.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <param name="loggingSource">Logging source. Optional.</param>
        /// <returns>Object or null if cannot be resolved.</returns>
        protected object ResolveObject(Type type, string loggingSource = "")
        {
            return UseInternalObjectResolver ?
                TypeHelpers.ResolveObjectForType(type, Resolver, loggingSource) :
                Resolver(type);
        }

        private object[] GetAndResolveHandlerParameters(object obj, MethodBase handlerMethod)
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
                            paramsarr[i] = Resolver(parameters[i].ParameterType);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            paramsarr[i] = Resolver(parameters[i].ParameterType);
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
            return new[] { obj };
        }

        /// <summary>
        /// Execute method.
        /// </summary>
        /// <param name="handler">Handler.</param>
        /// <param name="obj">The first argument.</param>
        /// <param name="handlerMethod">Method to execute.</param>
        protected void ExecuteHandler(object handler, object obj, MethodBase handlerMethod)
        {
            handlerMethod.Invoke(handler, GetAndResolveHandlerParameters(obj, handlerMethod));
        }

        /// <summary>
        /// Execute method in async mode. Handler should return <see cref="Task" /> otherwise
        /// it will be executed in sync mode.
        /// </summary>
        /// <param name="handler">Async handler.</param>
        /// <param name="obj">The first argument.</param>
        /// <param name="handlerMethod">Method to execute.</param>
        protected async Task ExecuteHandlerAsync(object handler, object obj, MethodBase handlerMethod)
        {
            var task = handlerMethod.Invoke(handler, GetAndResolveHandlerParameters(obj, handlerMethod)) as Task;
            if (task != null)
            {
                await task;
            }
        }
    }
}
