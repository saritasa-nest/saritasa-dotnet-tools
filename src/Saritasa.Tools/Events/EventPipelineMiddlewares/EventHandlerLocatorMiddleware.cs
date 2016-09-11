// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Events.EventPipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Linq.Expressions;
    using Messages;
    using Internal;

    /// <summary>
    /// Locate command hanlder.
    /// </summary>
    public class EventHandlerLocatorMiddleware : IMessagePipelineMiddleware
    {
        const string HandlerPrefix = "Handle";

        private Assembly[] assemblies;

        private IList<MethodInfo> eventHandlers = null;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="assemblies">Assemblies to locate.</param>
        public EventHandlerLocatorMiddleware(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length < 1)
            {
                throw new ArgumentException("Assemblies to search handlers were not specified");
            }
            if (assemblies.Any(a => a == null))
            {
                throw new ArgumentNullException("Assemblies contain null value");
            }
            this.assemblies = assemblies;

            // precache all types with event handlers
            eventHandlers = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.GetTypeInfo().GetCustomAttribute<EventHandlersAttribute>() != null)
                .SelectMany(t => t.GetTypeInfo().GetMethods())
                .Where(m => m.Name.StartsWith(HandlerPrefix))
                .ToArray();
        }

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException("Message should be EventMessage type");
            }

            // find handler methods
            var eventtype = eventMessage.Content.GetType();
            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug($"Finding command handler for type {eventtype.Name}", nameof(EventHandlerLocatorMiddleware));
            }
            var methods = eventHandlers
                .Where(m => m.GetParameters().Any(pt => pt.ParameterType == eventtype))
                .ToList();
            if (methods == null)
            {
                methods = new List<MethodInfo>();
            }
            var selfMethod = eventtype.GetTypeInfo().GetMethod(HandlerPrefix);
            if (selfMethod != null)
            {
                methods.Add(selfMethod);
            }
            if (InternalLogger.IsDebugEnabled)
            {
                if (methods.Any())
                {
                    for (int i = 0; i < methods.Count; i++)
                    {
                        InternalLogger.Debug($"Found \"{methods[i].Name}\" for event {eventtype}",
                            nameof(EventHandlerLocatorMiddleware));
                    }
                }
                else
                {
                    InternalLogger.Debug($"No handlers found for event {eventtype}");
                }
            }
            eventMessage.HandlerMethods = methods;
        }
    }
}
