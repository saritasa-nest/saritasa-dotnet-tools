// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    /// <summary>
    /// Resolve event handlers.
    /// </summary>
    public class EventHandlerResolverMiddleware : BaseHandlerResolverMiddleware,
        IMessagePipelineMiddleware, IMessagePipelinePostAction
    {
        /// <inheritdoc />
        public string Id { get; set; } = nameof(EventHandlerResolverMiddleware);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="useInternalObjectResolver">Use internal object resolver for handlers.
        /// Otherwise <see cref="IServiceProvider" /> will be used. <c>True</c> by default.</param>
        public EventHandlerResolverMiddleware(bool useInternalObjectResolver = true) : base(useInternalObjectResolver)
        {
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            var handlerMethods = (EventHandlerMethodWithObject[])
                messageContext.Items[EventHandlerLocatorMiddleware.HandlerMethodsKey];
            var handlerObjects = new object[handlerMethods.Length];

            for (int i = 0; i < handlerMethods.Length; i++)
            {
                object handler = null;
                // Event already implements Handle method within it.
                if (handlerMethods[i].Method.DeclaringType == messageContext.Content.GetType())
                {
                    handler = messageContext.Content;
                }
                if (handler == null && handlerMethods[i].Object != null)
                {
                    handler = handlerMethods[i].Object;
                }
                if (handler == null)
                {
                    try
                    {
                        if (UseInternalObjectResolver)
                        {
                            handler = CreateHandlerWithCache(handlerMethods[i].Method.DeclaringType,
                                messageContext.ServiceProvider);
                        }
                        else
                        {
                            handler = messageContext.ServiceProvider.GetService(handlerMethods[i].Method.DeclaringType);
                        }
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Info(string.Format(Properties.Strings.ExceptionOnResolve,
                            handlerMethods[i].Method.Name, ex), nameof(EventHandlerExecutorMiddleware));
                    }
                }
                if (handler == null)
                {
                    InternalLogger.Warn(string.Format(Properties.Strings.CannotResolveHandler,
                        handlerMethods[i].Method.Name), nameof(EventHandlerExecutorMiddleware));
                }
                handlerObjects[i] = handler;
            }

            messageContext.Items[HandlerObjectKey] = handlerObjects;
        }

        /// <inheritdoc />
        public void PostHandle(IMessageContext messageContext)
        {
            // Release handler.
            if (UseInternalObjectResolver)
            {
                var handlers = messageContext.Items[HandlerObjectKey] as object[];
                for (int i = 0; i < handlers.Length; i++)
                {
                    var disposable = handlers[i] as IDisposable;
                    disposable?.Dispose();
                }
            }
        }
    }
}
