// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Events.EventPipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Messages;
    using Internal;

    /// <summary>
    /// Default event executor. It does not process events with Rejected status.
    /// </summary>
    public class EventExecutorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "EventExecutor";

        readonly Func<Type, object> resolver;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">DI resolver.</param>
        public EventExecutorMiddleware(Func<Type, object> resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }
            this.resolver = resolver;
        }

        void ExecuteHandler(object handler, object @event, MethodBase handlerMethod)
        {
            var parameters = handlerMethod.GetParameters();
            var paramsarr = new object[parameters.Length];
            if (parameters.Length > 1)
            {
                if (handlerMethod.DeclaringType != @event.GetType())
                {
                    paramsarr[0] = @event;
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        paramsarr[i] = resolver(parameters[i].ParameterType);
                    }
                }
                else
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        paramsarr[i] = resolver(parameters[i].ParameterType);
                    }
                }
                handlerMethod.Invoke(handler, paramsarr);
            }
            else
            {
                if (parameters.Length == 1)
                {
                    paramsarr[0] = @event;
                }
                handlerMethod.Invoke(handler, paramsarr);
            }
        }

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException("Message should be EventMessage type");
            }

            // rejected events are not needed to process
            if (eventMessage.Status == Message.ProcessingStatus.Rejected)
            {
                return;
            }

            var exceptions = new List<Exception>(3);
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < eventMessage.HandlerMethods.Count; i++)
            {
                object handler = null;
                if (eventMessage.HandlerMethods[i].DeclaringType == eventMessage.Content.GetType())
                {
                    handler = eventMessage.Content;
                }
                if (handler == null)
                {
                    try
                    {
                        handler = resolver(eventMessage.HandlerMethods[i].DeclaringType);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Info($"Exception while resolving {eventMessage.HandlerMethods[i].Name}: {ex}");
                    }
                }
                if (handler == null)
                {
                    handler = TypeHelpers.ResolveObjectForType(eventMessage.HandlerMethods[i].DeclaringType,
                        resolver, nameof(EventExecutorMiddleware));
                }
                if (handler == null)
                {
                    InternalLogger.Warn($"Cannot resolve handler {eventMessage.HandlerMethods[i].Name}");
                    continue;
                }

                // invoke method and resolve parameters if needed
                try
                {
                    ExecuteHandler(handler, eventMessage.Content, eventMessage.HandlerMethods[i]);
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn($"Exception while process \"{handler}\": {ex}",
                        nameof(EventExecutorMiddleware));
                    var innerException = ex.InnerException;
                    if (innerException != null)
                    {
                        exceptions.Add(innerException);
                    }
                    else
                    {
                        InternalLogger.Warn($"For some reason InnerException is null. Type: {ex.GetType()}.",
                            nameof(EventExecutorMiddleware));
                    }
                }
            }
            stopWatch.Stop();
            if (exceptions.Count > 0)
            {
                eventMessage.Error = new AggregateException(exceptions);
                eventMessage.ErrorMessage = string.Join(";", exceptions.Select(e => e.Message));
            }
            eventMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
            eventMessage.Status = Message.ProcessingStatus.Completed;
        }
    }
}
