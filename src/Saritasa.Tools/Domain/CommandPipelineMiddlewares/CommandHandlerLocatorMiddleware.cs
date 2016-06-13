// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain.CommandPipelineMiddlewares
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

        private IDictionary<Type, Tuple<Type, MethodInfo>> cache =
            new Dictionary<Type, Tuple<Type, MethodInfo>>(150);

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="assemblies">Assemblies to locate.</param>
        public CommandHandlerLocatorMiddleware(params Assembly[] assemblies)
        {
            this.assemblies = assemblies;
        }

        /// <inheritdoc />
        public void Handle(CommandExecutionContext context)
        {
            var cmdtype = context.Command.GetType();
            var clstypes = assemblies.SelectMany(a => a.GetTypes()).Where(t => typeof(ICommandHandler).IsAssignableFrom(t));
            var method = clstypes
                .SelectMany(t => t.GetMethods())
                .FirstOrDefault(m => m.Name.StartsWith("Handle") && m.GetParameters().Any(pt => pt.ParameterType == cmdtype));
            context.HandlerMethod = method;
            context.HandlerType = method.DeclaringType;
        }
    }
}
