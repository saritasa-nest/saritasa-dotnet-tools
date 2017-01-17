// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Abstractions;
    using Common;
    using Internal;

    /// <summary>
    /// Locates command hanlder.
    /// </summary>
    public class EventHandlerLocatorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "EventLocator";

        const string HandlerPrefix = "Handle";

        readonly Assembly[] assemblies;

        IList<MethodInfo> eventHandlers;

        HandlerSearchMethod handlerSearchMethod = HandlerSearchMethod.ClassAttribute;

        /// <summary>
        /// What method to use to search command handler class.
        /// </summary>
        public HandlerSearchMethod HandlerSearchMethod
        {
            get
            {
                return handlerSearchMethod;
            }

            set
            {
                if (handlerSearchMethod != value)
                {
                    handlerSearchMethod = value;
                    Init();
                }
            }
        }

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
                throw new ArgumentNullException(nameof(assemblies));
            }
            this.assemblies = assemblies;
            Init();
        }

        private void Init()
        {
            // precache all types with event handlers
            eventHandlers = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => HandlerSearchMethod == HandlerSearchMethod.ClassAttribute ?
                    t.GetTypeInfo().GetCustomAttribute<EventHandlersAttribute>() != null :
                    t.Name.EndsWith("Handlers"))
                .SelectMany(t => t.GetTypeInfo().GetMethods())
                .Where(m => m.Name.StartsWith(HandlerPrefix))
                .ToArray();
        }

        /// <inheritdoc />
        public virtual void Handle(IMessage message)
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
            var selfMethod = eventtype.GetTypeInfo().GetMethod(HandlerPrefix);
            if (selfMethod != null)
            {
                methods.Add(selfMethod);
            }
            if (InternalLogger.IsDebugEnabled)
            {
                if (methods.Any())
                {
                    for (var i = 0; i < methods.Count; i++)
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

            if (eventMessage.HandlerMethods == null)
            {
                eventMessage.HandlerMethods = methods;
            }
            else
            {
                eventMessage.HandlerMethods.AddRange(methods);
            }
        }
    }
}
