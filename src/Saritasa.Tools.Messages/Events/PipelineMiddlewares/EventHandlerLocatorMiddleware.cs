﻿// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    /// <summary>
    /// Locates event handler.
    /// </summary>
    public class EventHandlerLocatorMiddleware : BaseHandlerLocatorMiddleware
    {
        const string HandlerPrefix = "Handle";

        readonly Assembly[] assemblies;

        IList<MethodInfo> eventHandlers;

        /// <inheritdoc />
        public EventHandlerLocatorMiddleware(IDictionary<string, string> dict) : base(dict)
        {
        }

        /// <summary>
        /// .ctor
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
        public override void Handle(IMessage message)
        {
            var eventMessage = message as EventMessage;
            if (eventMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(EventMessage)));
            }

            // Find handler methods.
            var eventtype = eventMessage.Content.GetType();
            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug(string.Format(Properties.Strings.SearchEventHandler, eventtype.Name),
                    nameof(EventHandlerLocatorMiddleware));
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
