// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;
    using System.Reflection;
    using Messages;
    using Internal;

    /// <summary>
    /// Default command executor. It does not process commands with Rejected status.
    /// </summary>
    public class CommandExecutorMiddleware : IMessagePipelineMiddleware
    {
        Func<Type, object> resolver;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">DI resolver.</param>
        public CommandExecutorMiddleware(Func<Type, object> resolver)
        {
            this.resolver = resolver;
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
                    catch (Exception ex)
                    {
                        InternalLogger.Error($"Cannot set value for handler {handler} of type {handlerType}. {ex}",
                            nameof(CommandExecutorMiddleware));
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
        public void Handle(Message message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException("Message should be CommandMessage type");
            }

            // rejected commands are not needed to process
            if (commandMessage.Status == Message.ProcessingStatus.Rejected)
            {
                return;
            }

            object handler = null;
            bool canResolve = true;

            // we should have handler object first
            try
            {
                handler = resolver(commandMessage.HandlerType);
            }
            catch (Exception)
            {
                canResolve = false;
                handler = CreateFromDefaultCtorAndResolve(commandMessage.HandlerType);
            }

            if (handler == null)
            {
                handler = CreateFromDefaultCtorAndResolve(commandMessage.HandlerType);
            }

            // if we don't have handler - throw exception
            if (handler == null)
            {
                var exception = new CommandHandlerNotFoundException();
                exception.Data.Add(nameof(canResolve), canResolve);
                commandMessage.Status = Message.ProcessingStatus.Rejected;
                throw exception;
            }

            // invoke method and resolve parameters if needed
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                ExecuteHandler(handler, commandMessage.Content, commandMessage.HandlerMethod);
                stopWatch.Stop();
                commandMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
                commandMessage.Status = Message.ProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn($"Exception while process \"{handler}\": {ex}", nameof(CommandExecutorMiddleware));
                stopWatch.Stop();
                commandMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
                commandMessage.Status = Message.ProcessingStatus.Failed;
                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    commandMessage.Error = innerException;
                    commandMessage.ErrorDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(innerException);
                }
                else
                {
                    InternalLogger.Warn($"For some reason InnerException is null. Type: {ex.GetType()}.", nameof(CommandExecutorMiddleware));
                }
            }
        }
    }
}
