// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class EventHandlerExecutorMiddleware : BaseHandlerExecutorMiddleware,
        IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware
    {
        /// <summary>
        /// Middleware identifier.
        /// </summary>
        public string Id { get; set; } = nameof(EventHandlerExecutorMiddleware);

        /// <summary>
        /// Include execution duration.
        /// </summary>
        public bool IncludeExecutionDuration { get; set; } = true;

        private async Task InternalHandle(IMessageContext messageContext, CancellationToken cancellationToken,
            bool async = false)
        {
            var exceptions = new List<Exception>(3); // Stores exceptions from all handlers.
            var handlerMethods =
                messageContext.GetItemByKey<EventHandlerMethodWithObject[]>(EventHandlerLocatorMiddleware.HandlerMethodsKey);
            var handlers = messageContext.GetItemByKey<object[]>(BaseHandlerResolverMiddleware.HandlerObjectKey);

            // Executes every handle method.
            for (int i = 0; i < handlerMethods.Length; i++)
            {
                object handler = handlers[i];
                if (handler == null)
                {
                    continue;
                }

                // Invoke method and resolve parameters if needed.
                Stopwatch stopwatch = null;
                if (IncludeExecutionDuration)
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }

                try
                {
                    if (async)
                    {
                        await ExecuteHandlerAsync(handler, messageContext.Content, messageContext.ServiceProvider,
                            handlerMethods[i].Method);
                    }
                    else
                    {
                        ExecuteHandler(handler, messageContext.Content, messageContext.ServiceProvider,
                            handlerMethods[i].Method);
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess, handler, ex),
                        nameof(EventHandlerExecutorMiddleware));
                    var innerException = ex.InnerException;
                    if (innerException != null)
                    {
                        exceptions.Add(innerException);
                    }
                    else
                    {
                        InternalLogger.Warn(string.Format(Properties.Strings.InnerExceptionIsNull, ex.GetType()),
                            nameof(EventHandlerExecutorMiddleware));
                    }
                }
                finally
                {
                    if (stopwatch != null)
                    {
                        stopwatch.Stop();
                        messageContext.Items[MessageContextConstants.ExecutionDurationKey] = (int)stopwatch.ElapsedMilliseconds;
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            if (exceptions.Count > 0)
            {
                messageContext.FailException = new AggregateException(exceptions);
            }
            messageContext.Status = ProcessingStatus.Completed;
        }

        /// <inheritdoc />
        public void Handle(IMessageContext messageContext)
        {
            // Rejected events are not needed to process.
            if (messageContext.Status == ProcessingStatus.Rejected)
            {
                return;
            }

            // It will be sync anyway but simplified for better performance.
#pragma warning disable 4014
            InternalHandle(messageContext, CancellationToken.None, async: false);
#pragma warning restore 4014
        }

        /// <inheritdoc />
        public async Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            // Rejected events are not needed to process.
            if (messageContext.Status == ProcessingStatus.Rejected)
            {
                return;
            }

            await InternalHandle(messageContext, cancellationToken, async: true);
        }
    }
}
