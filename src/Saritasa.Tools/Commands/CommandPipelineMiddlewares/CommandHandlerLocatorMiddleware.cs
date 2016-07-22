// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Messages;

    /// <summary>
    /// Locate command hanlder.
    /// </summary>
    public class CommandHandlerLocatorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id
        {
            get { return "locator-by-assembly"; }
        }

        private Assembly[] assemblies;

        // TODO: [IK] need to implement caching to improve speed
        private IDictionary<Type, Tuple<Type, MethodInfo>> cache =
            new System.Collections.Concurrent.ConcurrentDictionary<Type, Tuple<Type, MethodInfo>>(4, 150);

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="assemblies">Assemblies to locate.</param>
        public CommandHandlerLocatorMiddleware(params Assembly[] assemblies)
        {
            this.assemblies = assemblies;
        }

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException("Message should be CommandMessage type");
            }

            var cmdtype = commandMessage.Content.GetType();
            var clstypes = assemblies.SelectMany(a => a.GetTypes()).Where(t => t.GetTypeInfo().GetCustomAttribute<CommandsHandlerAttribute>() != null);
            var method = clstypes
                .SelectMany(t => t.GetTypeInfo().GetMethods())
                .FirstOrDefault(m => m.Name.StartsWith("Handle") && m.GetParameters().Any(pt => pt.ParameterType == cmdtype));
            if (method == null)
            {
                throw new CommandHandlerNotFoundException();
            }
            commandMessage.HandlerMethod = method;
            commandMessage.HandlerType = method.DeclaringType;
        }
    }
}
