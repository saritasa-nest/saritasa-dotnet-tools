﻿// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Commands.PipelineMiddlewares
{
    /// <summary>
    /// Locate command hanlder.
    /// </summary>
    public class CommandHandlerLocatorMiddleware : BaseHandlerLocatorMiddleware
    {
        const string HandlerPrefix = "Handle";

        /// <summary>
        /// Commands methods cache. Type is for command type, MethodInfo is for actual handler.
        /// </summary>
        readonly System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo> cache =
            new System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo>();

        ICollection<MethodInfo> commandHandlers;

        /// <inheritdoc />
        public CommandHandlerLocatorMiddleware(IDictionary<string, string> dict) : base(dict)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="assemblies">Assemblies to locate.</param>
        public CommandHandlerLocatorMiddleware(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length < 1)
            {
                throw new ArgumentException(Properties.Strings.AssembliesNotSpecified);
            }
            if (assemblies.Any(a => a == null))
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            this.Assemblies = assemblies;
            Initialize();
        }

        /// <summary>
        /// Prefills command handlers. We cannot do it in runtime because there can be race conditions
        /// during initialization. Much simple just do that once on application start.
        /// </summary>
        protected override void Initialize()
        {
            // Precache all types with command handlers.
            commandHandlers = Assemblies.SelectMany(a => a.GetTypes())
                .Where(t =>
                    HandlerSearchMethod == HandlerSearchMethod.ClassAttribute ?
                        t.GetTypeInfo().GetCustomAttribute<CommandHandlersAttribute>() != null :
                        t.Name.EndsWith("Handlers"))
                .SelectMany(t => t.GetTypeInfo().GetMethods())
                .Where(m => m.Name.StartsWith(HandlerPrefix))
                .ToArray();
            if (!commandHandlers.Any())
            {
                var assembliesStr = string.Join(",", Assemblies.Select(a => a.FullName));
                InternalLogger.Warn(string.Format(Properties.Strings.NoHandlersInAssembly, assembliesStr),
                    nameof(CommandHandlerLocatorMiddleware));
            }
        }

        /// <inheritdoc />
        public override void Handle(IMessage message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(CommandMessage)));
            }

            // Find handler method, first try to find cached value.
            var cmdtype = commandMessage.Content.GetType();
            var method = cache.GetOrAdd(cmdtype, (handlerCmdType) =>
            {
                return commandHandlers
                    .FirstOrDefault(m => m.GetParameters().Any(pt => pt.ParameterType == handlerCmdType));
            });

            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug(string.Format(Properties.Strings.SearchCommandHandler, cmdtype.Name), nameof(CommandHandlerLocatorMiddleware));
            }
            if (method == null)
            {
                method = cmdtype.GetTypeInfo().GetMethod(HandlerPrefix);
            }
            if (method == null)
            {
                var assembliesStr = string.Join(",", Assemblies.Select(a => a.FullName));
                InternalLogger.Warn(string.Format(Properties.Strings.SearchCommandHandlerNotFound, cmdtype.Name, assembliesStr),
                    nameof(CommandHandlerLocatorMiddleware));
                throw new CommandHandlerNotFoundException(cmdtype.Name);
            }
            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug(string.Format(Properties.Strings.CommandHandlerFound, method, cmdtype),
                    nameof(CommandHandlerLocatorMiddleware));
            }
            commandMessage.HandlerMethod = method;
            commandMessage.HandlerType = method.DeclaringType;
        }
    }
}
