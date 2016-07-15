// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Default command executor. It does not process commands with Rejected status.
    /// </summary>
    public class CommandExecutorMiddleware : ICommandPipelineMiddleware
    {
        /// <inheritdoc />
        public string Id
        {
            get { return "executor"; }
        }

        Func<Type, object> resolver;

        bool rethrow;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">DI resolver.</param>
        /// <param name="rethrow">Rethrow exceptions that occured during commands execution.</param>
        public CommandExecutorMiddleware(Func<Type, object> resolver, bool rethrow = true)
        {
            this.resolver = resolver;
            this.rethrow = rethrow;
        }

        private object CreateFromDefaultCtorAndResolve(Type handlerType)
        {
            object handler = null;
            var ctor = handlerType.GetTypeInfo().GetConstructor(new Type[] { });
            if (ctor != null)
            {
                handler = ctor.Invoke(null);
                var props = handler.GetType().GetTypeInfo().GetProperties(BindingFlags.Public);
                foreach (var prop in props)
                {
                    if (prop.GetValue(handler) != null)
                    {
                        continue;
                    }

                    try
                    {
                        prop.SetValue(handler, resolver(prop.DeclaringType));
                    }
                    catch (Exception)
                    {
                        // just skip, have no idea what to do
                    }
                }
            }
            return handler;
        }

        private void ExecuteHandler(object handler, object command, MethodInfo handlerMethod)
        {
            var parameters = handlerMethod.GetParameters();
            if (parameters.Length == 1)
            {
                handlerMethod.Invoke(handler, new object[] { command });
            }
            else if (parameters.Length > 1)
            {
                var paramsarr = new object[parameters.Length];
                paramsarr[0] = command;
                for (int i = 1; i < parameters.Length; i++)
                {
                    paramsarr[i] = resolver(parameters[i].ParameterType);
                }
                handlerMethod.Invoke(handler, paramsarr);
            }
        }

        /// <inheritdoc />
        public void Execute(CommandExecutionContext context)
        {
            // rejected commands are not needed to process
            if (context.Status == CommandExecutionContext.CommandStatus.Rejected)
            {
                return;
            }

            object handler = null;
            bool canResolve = true;

            // we should have handler object first
            try
            {
                handler = resolver(context.HandlerType);
            }
            catch (Exception)
            {
                canResolve = false;
                handler = CreateFromDefaultCtorAndResolve(context.HandlerType);
            }

            // if we don't have handler - throw exception
            if (handler == null)
            {
                var exception = new CommandHandlerNotFoundException();
                exception.Data.Add(nameof(canResolve), canResolve);
                context.Status = CommandExecutionContext.CommandStatus.Rejected;
                throw exception;
            }

            // invoke method and resolve parameters if needed
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                ExecuteHandler(handler, context.Command, context.HandlerMethod);
                stopWatch.Stop();
                context.ExecutionDuration = stopWatch.ElapsedMilliseconds;
                context.Status = CommandExecutionContext.CommandStatus.Completed;
            }
            catch (Exception)
            {
                stopWatch.Stop();
                context.ExecutionDuration = stopWatch.ElapsedMilliseconds;
                context.Status = CommandExecutionContext.CommandStatus.Failed;
                if (this.rethrow)
                {
                    throw;
                }
            }
        }
    }
}
