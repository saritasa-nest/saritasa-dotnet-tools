// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;

    /// <summary>
    /// Locate command hanlder.
    /// </summary>
    public class CommandHandlerLocatorMiddleware : ICommandPipelineMiddleware
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
        public void Execute(CommandExecutionContext context)
        {
            var cmdtype = context.Command.GetType();
            var clstypes = assemblies.SelectMany(a => a.GetTypes()).Where(t => t.GetTypeInfo().GetCustomAttribute<CommandHandlerAttribute>() != null);
            var method = clstypes
                .SelectMany(t => t.GetTypeInfo().GetMethods())
                .FirstOrDefault(m => m.Name.StartsWith("Execute") && m.GetParameters().Any(pt => pt.ParameterType == cmdtype));
            context.HandlerMethod = method;
            context.HandlerType = method.DeclaringType;
        }
    }
}
