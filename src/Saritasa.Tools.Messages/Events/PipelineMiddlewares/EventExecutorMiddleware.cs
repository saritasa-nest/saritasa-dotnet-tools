// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstractions;
    using Common;
    using Internal;

    /// <summary>
    /// Default event executor. It does not process events with Rejected status.
    /// </summary>
    public class EventExecutorMiddleware : BaseExecutorMiddleware
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">DI resolver.</param>
        public EventExecutorMiddleware(Func<Type, object> resolver) : base(resolver)
        {
            Id = "EventExecutor";
        }

        private async Task InternalHandle(EventMessage eventMessage, bool async = false)
        {
            var exceptions = new List<Exception>(3); // stores exceptions from all handlers
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            // executes every handle method
            for (int i = 0; i < eventMessage.HandlerMethods.Count; i++)
            {
                object handler = null;
                // event already implements Handle method
                if (eventMessage.HandlerMethods[i].DeclaringType == eventMessage.Content.GetType())
                {
                    handler = eventMessage.Content;
                }
                if (handler == null)
                {
                    try
                    {
                        handler = ResolveObject(eventMessage.HandlerMethods[i].DeclaringType, nameof(EventExecutorMiddleware));
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Info($"Exception while resolving {eventMessage.HandlerMethods[i].Name}: {ex}");
                    }
                }
                if (handler == null)
                {
                    InternalLogger.Warn($"Cannot resolve handler {eventMessage.HandlerMethods[i].Name}");
                    continue;
                }

                // invoke method and resolve parameters if needed
                try
                {
                    if (async)
                    {
                        await ExecuteHandlerAsync(handler, eventMessage.Content, eventMessage.HandlerMethods[i]);
                    }
                    else
                    {
                        ExecuteHandler(handler, eventMessage.Content, eventMessage.HandlerMethods[i]);
                    }
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
            eventMessage.Status = ProcessingStatus.Completed;
        }

        /// <inheritdoc />
        public override void Handle(IMessage message)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException("Message should be EventMessage type");
            }

            // rejected events are not needed to process
            if (eventMessage.Status == ProcessingStatus.Rejected)
            {
                return;
            }

            // it will be sync anyway but simplified for better performance
#pragma warning disable 4014
            InternalHandle(eventMessage, async: false);
#pragma warning restore 4014
        }

        /// <inheritdoc />
        public override async Task HandleAsync(IMessage message)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException("Message should be EventMessage type");
            }

            // rejected events are not needed to process
            if (eventMessage.Status == ProcessingStatus.Rejected)
            {
                return;
            }

            await InternalHandle(eventMessage, async: true);
        }
    }
}
