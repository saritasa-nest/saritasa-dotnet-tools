// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    /// <summary>
    /// The middleware is to locate event handler.
    /// </summary>
    public class EventHandlerLocatorMiddleware : BaseHandlerLocatorMiddleware,
        IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = nameof(EventHandlerLocatorMiddleware);

        private const string HandlerPrefix = "Handle";

        internal const string HandlerMethodsKey = "handler-methods";

        private readonly Assembly[] assemblies;

        private MethodInfo[] eventHandlers;

        private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo[]> cache =
            new System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo[]>();

        /// <inheritdoc />
        public EventHandlerLocatorMiddleware(IDictionary<string, string> dict) : base(dict)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assemblies">Assemblies to locate.</param>
        public EventHandlerLocatorMiddleware(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length < 1)
            {
                throw new ArgumentException(Properties.Strings.AssembliesNotSpecified);
            }
            if (assemblies.Any(a => a == null))
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            this.assemblies = assemblies;
            Initialize();
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            // Precache all types with event handlers.
            eventHandlers = assemblies.SelectMany(a => a.GetTypes())
                .Where(t =>
                    HandlerSearchMethod == HandlerSearchMethod.ClassAttribute
                        ? t.GetTypeInfo().GetCustomAttribute<EventHandlersAttribute>() != null
                        : t.Name.EndsWith("Handlers"))
                .SelectMany(t => t.GetTypeInfo().GetMethods())
                .Where(m => m.Name.StartsWith(HandlerPrefix))
                .ToArray();
        }

        /// <inheritdoc />
        public void Handle(IMessageContext messageContext)
        {
            // Find handler methods.
            var eventType = messageContext.Content.GetType();
            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug(string.Format(Properties.Strings.SearchEventHandler, eventType.Name),
                    nameof(EventHandlerLocatorMiddleware));
            }
            var methods = cache.GetOrAdd(eventType, FindOrCreateMethodHandlers);
            if (InternalLogger.IsDebugEnabled)
            {
                DumpFoundMethods(methods, eventType);
            }

            messageContext.Items.TryGetValue(HandlerMethodsKey, out object handlersObj);
            var methodsWrap = methods.Select(m => new EventHandlerMethodWithObject(m)).ToArray();
            var handlers = handlersObj as EventHandlerMethodWithObject[];
            messageContext.Items[HandlerMethodsKey] =
                ArrayHelpers.AddItems(handlers, methodsWrap);
        }

        private void DumpFoundMethods(MethodInfo[] methods, Type eventtype)
        {
            if (methods.Any())
            {
                for (var i = 0; i < methods.Length; i++)
                {
                    InternalLogger.Debug(
                        string.Format(Properties.Strings.EventHandlerFound, Properties.Strings.EventHandlerFound,
                            methods[i].Name, eventtype), nameof(EventHandlerLocatorMiddleware));
                }
            }
            else
            {
                InternalLogger.Debug(string.Format(Properties.Strings.EventHandlerNotFound, eventtype),
                    nameof(EventHandlerLocatorMiddleware));
            }
        }

        private MethodInfo[] FindOrCreateMethodHandlers(Type eventType)
        {
            var eventTypeInfo = eventType.GetTypeInfo();

            // Non-generic events lookup.
            var methods = new List<MethodInfo>();
            if (!eventTypeInfo.IsGenericType)
            {
                methods.AddRange(
                    eventHandlers
                        .Where(m => m.GetParameters().Any(pt => pt.ParameterType == eventType))
                        .ToList());
            }

            // If event has its own handler.
            var selfMethod = eventType.GetTypeInfo().GetMethod(HandlerPrefix);
            if (selfMethod != null)
            {
                methods.Add(selfMethod);
            }

            // For generic event we should find suitable method and make generic method.
            if (eventTypeInfo.IsGenericType)
            {
                var eventGenericType = eventTypeInfo.GetGenericTypeDefinition();
                var genericEventMethods = eventHandlers
                    .Where(m =>
                        m.IsGenericMethod &&
                        m.GetParameters().Any(pt => pt.ParameterType.GetGenericTypeDefinition() == eventGenericType));
                if (genericEventMethods != null && genericEventMethods.Any())
                {
                    methods.AddRange(
                        genericEventMethods.Select(m => m.MakeGenericMethod(eventType.GetTypeInfo().GetGenericArguments())
                    ));
                }
            }
            return methods.ToArray();
        }
    }
}
