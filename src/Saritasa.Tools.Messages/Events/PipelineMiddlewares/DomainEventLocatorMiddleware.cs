// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Abstractions;
    using Domain;
    using Common;

    /// <summary>
    /// Uses domain events manager to raise events.
    /// </summary>
    public class DomainEventLocatorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "DomainEventLocator";

        readonly IDomainEventsManager eventsManager;

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
            this.eventsManager = eventsManager;
        }

        /// <inheritdoc />
        public void Handle(IMessage message)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException("Message should be EventMessage type");
            }

            var hasHandlersGenericMethod = typeof(IDomainEventsManager).GetTypeInfo().GetMethod("HasHandlers").MakeGenericMethod(message.Content.GetType());
            if ((bool)hasHandlersGenericMethod.Invoke(eventsManager, new object[] { }))
            {
                var raiseGenericMethod = typeof(IDomainEventsManager).GetTypeInfo().GetMethod("Raise").MakeGenericMethod(message.Content.GetType());
                if (eventMessage.HandlerMethods == null)
                {
                    eventMessage.HandlerMethods = new List<MethodInfo>() { raiseGenericMethod };
                }
                else
                {
                    eventMessage.HandlerMethods.Add(raiseGenericMethod);
                }
            }
        }
    }
}
