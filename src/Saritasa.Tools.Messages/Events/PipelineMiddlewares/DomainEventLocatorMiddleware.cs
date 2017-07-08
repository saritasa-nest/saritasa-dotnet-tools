// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Domain;

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
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
        /// <param name="dict">Parameters dictionary.</param>
        public DomainEventLocatorMiddleware(IDictionary<string, string> dict)
        {
            throw new NotSupportedException("The middleware does not support instantiation from dict.");
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
            this.eventsManager = eventsManager;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessage message)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(EventMessage)));
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
