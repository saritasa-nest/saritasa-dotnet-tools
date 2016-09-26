// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Messages;
    using Internal;
    using System.Linq.Expressions;

    /// <summary>
    /// Locate command hanlder.
    /// </summary>
    public class CommandHandlerLocatorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "CommandHandlerLocator";

        const string HandlerPrefix = "Handle";

        readonly Assembly[] assemblies;

        // TODO: [IK] need to implement caching to improve speed
        IDictionary<Type, Expression<Func<object>>> cache =
            new System.Collections.Concurrent.ConcurrentDictionary<Type, Expression<Func<object>>>(4, 150);

        readonly IList<MethodInfo> commandHandlers = null;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="assemblies">Assemblies to locate.</param>
        public CommandHandlerLocatorMiddleware(params Assembly[] assemblies)
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

            // precache all types with command handlers
            commandHandlers = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.GetTypeInfo().GetCustomAttribute<CommandHandlersAttribute>() != null)
                .SelectMany(t => t.GetTypeInfo().GetMethods())
                .Where(m => m.Name.StartsWith(HandlerPrefix))
                .ToArray();
            if (commandHandlers.Any() == false)
            {
                var assembliesStr = string.Join(",", assemblies.Select(a => a.FullName));
                InternalLogger.Warn($"Cannot find command handlers in assemblies {assembliesStr}",
                    nameof(CommandHandlerLocatorMiddleware));
                throw new CommandHandlerNotFoundException();
            }
        }

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException("Message should be CommandMessage type");
            }

            // find handler method
            var cmdtype = commandMessage.Content.GetType();
            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug($"Finding command handler for type {cmdtype.Name}", nameof(CommandHandlerLocatorMiddleware));
            }
            var method = commandHandlers
                .FirstOrDefault(m => m.GetParameters().Any(pt => pt.ParameterType == cmdtype));
            if (method == null)
            {
                method = cmdtype.GetTypeInfo().GetMethod(HandlerPrefix);
            }
            if (method == null)
            {
                var assembliesStr = string.Join(",", assemblies.Select(a => a.FullName));
                InternalLogger.Warn($"Cannot find command handler for command {cmdtype.Name} in assemblies {assembliesStr}",
                    nameof(CommandHandlerLocatorMiddleware));
                throw new CommandHandlerNotFoundException();
            }
            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug($"Found \"{method}\" for command {cmdtype}", nameof(CommandHandlerLocatorMiddleware));
            }
            commandMessage.HandlerMethod = method;
            commandMessage.HandlerType = method.DeclaringType;
        }
    }
}
