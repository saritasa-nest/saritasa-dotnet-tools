// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
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
        public EventExecutorMiddleware() : base()
        {
            Id = this.GetType().Name;
        }

        private async Task InternalHandle(IMessageContext messageContext, CancellationToken cancellationToken,
            bool async = false)
        {
            var exceptions = new List<Exception>(3); // Stores exceptions from all handlers.
            var handlerMethods = (MethodInfo[])messageContext.Items[EventHandlerLocatorMiddleware.HandlerMethodsKey];

            // Executes every handle method.
            for (int i = 0; i < handlerMethods.Length; i++)
            {
                object handler = null;
                // Event already implements Handle method.
                if (handlerMethods[i].DeclaringType == messageContext.Content.GetType())
                {
                    handler = messageContext.Content;
                }
                if (handler == null)
                {
                    try
                    {
                        handler = ResolveObject(handlerMethods[i].DeclaringType, messageContext.ServiceProvider,
                            nameof(EventExecutorMiddleware));
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Info(string.Format(Properties.Strings.ExceptionOnResolve,
                            handlerMethods[i].Name, ex), nameof(EventExecutorMiddleware));
                    }
                }
                if (handler == null)
                {
                    InternalLogger.Warn(string.Format(Properties.Strings.CannotResolveHandler,
                        handlerMethods[i].Name), nameof(EventExecutorMiddleware));
                    continue;
                }

                // Invoke method and resolve parameters if needed.
                try
                {
                    if (async)
                    {
                        await ExecuteHandlerAsync(handler, messageContext.Content, messageContext.ServiceProvider,
                            handlerMethods[i]);
                    }
                    else
                    {
                        ExecuteHandler(handler, messageContext.Content, messageContext.ServiceProvider, handlerMethods[i]);
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
            if (exceptions.Count > 0)
            {
                messageContext.FailException = new AggregateException(exceptions);
            }
            messageContext.Status = ProcessingStatus.Completed;
        }

        /// <inheritdoc />
        public override void Handle(IMessageContext messageContext)
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
        public override async Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
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
