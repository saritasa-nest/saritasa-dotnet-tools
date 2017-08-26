// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Domain;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    /// <summary>
    /// Uses domain events manager to raise events.
    /// </summary>
    public class DomainEventLocatorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = nameof(DomainEventLocatorMiddleware);

        private readonly IDomainEventsManager eventsManager;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public DomainEventLocatorMiddleware(IDictionary<string, string> parameters)
        {
            throw new NotSupportedException("The middleware does not support instantiation from parameters dictionary.");
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="eventsManager">Domain events manager.</param>
        public DomainEventLocatorMiddleware(IDomainEventsManager eventsManager)
        {
            if (eventsManager == null)
            {
                throw new ArgumentNullException(nameof(eventsManager));
            }
            this.Id = GetType().Name;
            this.eventsManager = eventsManager;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            var hasHandlersGenericMethod = typeof(IDomainEventsManager).GetTypeInfo().GetMethod("HasHandlers")
                .MakeGenericMethod(messageContext.Content.GetType());
            if ((bool)hasHandlersGenericMethod.Invoke(eventsManager, new object[] { }))
            {
                var raiseGenericMethod = eventsManager.GetType().GetTypeInfo().GetMethod("Raise")
                    .MakeGenericMethod(messageContext.Content.GetType());

                messageContext.Items.TryGetValue(EventHandlerLocatorMiddleware.HandlerMethodsKey, out object handlersObj);
                var handlers = handlersObj as EventHandlerMethodWithObject[];
                messageContext.Items[EventHandlerLocatorMiddleware.HandlerMethodsKey] =
                    ArrayHelpers.AddItem(handlers, new EventHandlerMethodWithObject(raiseGenericMethod, eventsManager));
            }
        }
    }
}
