// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    /// <summary>
    /// Default event executor. It does not process events with Rejected status.
    /// </summary>
    public class EventExecutorMiddleware : BaseExecutorMiddleware
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public EventExecutorMiddleware(IDictionary<string, string> parameters) : base(parameters)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">DI resolver.</param>
        public EventExecutorMiddleware(Func<Type, object> resolver) : base(resolver)
        {
            Id = "EventExecutor";
        }

        private async Task InternalHandle(EventMessage eventMessage, CancellationToken cancellationToken, bool async = false)
        {
            var exceptions = new List<Exception>(3); // Stores exceptions from all handlers.
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            // Executes every handle method.
            for (int i = 0; i < eventMessage.HandlerMethods.Count; i++)
            {
                object handler = null;
                // Event already implements Handle method.
                if (eventMessage.HandlerMethods[i].DeclaringType == eventMessage.Content.GetType())
                {
                    handler = eventMessage.Content;
                }
                if (handler == null)
                {
                    try
                    {
                        handler = ResolveObject(eventMessage.HandlerMethods[i].DeclaringType,
                            nameof(EventExecutorMiddleware));
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Info(string.Format(Properties.Strings.ExceptionOnResolve,
                            eventMessage.HandlerMethods[i].Name, ex), nameof(EventExecutorMiddleware));
                    }
                }
                if (handler == null)
                {
                    InternalLogger.Warn(string.Format(Properties.Strings.CannotResolveHandler,
                        eventMessage.HandlerMethods[i].Name), nameof(EventExecutorMiddleware));
                    continue;
                }

                // Invoke method and resolve parameters if needed.
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
                    InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess, handler, ex),
                        nameof(EventExecutorMiddleware));
                    var innerException = ex.InnerException;
                    if (innerException != null)
                    {
                        exceptions.Add(innerException);
                    }
                    else
                    {
                        InternalLogger.Warn(string.Format(Properties.Strings.InnerExceptionIsNull, ex.GetType()),
                            nameof(EventExecutorMiddleware));
                    }
                }
                finally
                {
                    // Release handler.
                    var disposable = handler as IDisposable;
                    disposable?.Dispose();
                }
                cancellationToken.ThrowIfCancellationRequested();
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
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(EventMessage)));
            }

            // Rejected events are not needed to process.
            if (eventMessage.Status == ProcessingStatus.Rejected)
            {
                return;
            }

            // It will be sync anyway but simplified for better performance.
#pragma warning disable 4014
            InternalHandle(eventMessage, CancellationToken.None, async: false);
#pragma warning restore 4014
        }

        /// <inheritdoc />
        public override async Task HandleAsync(IMessage message, CancellationToken cancellationToken)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(EventMessage)));
            }

            // Rejected events are not needed to process.
            if (eventMessage.Status == ProcessingStatus.Rejected)
            {
                return;
            }

            await InternalHandle(eventMessage, cancellationToken, async: true);
        }
    }
}
