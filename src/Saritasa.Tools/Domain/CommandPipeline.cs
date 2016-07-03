// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Commands processing pipeline.
    /// </summary>
    public class CommandPipeline
    {
        /// <summary>
        /// Middlewares list.
        /// </summary>
        protected IList<ICommandPipelineMiddleware> Middlewares { get; set; } = new List<ICommandPipelineMiddleware>();

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        [System.Diagnostics.DebuggerHidden]
        public void Execute(object command)
        {
            var context = new CommandExecutionContext(command);

            foreach (var handler in Middlewares)
            {
                handler.Handle(context);
            }
        }

        /// <summary>
        /// Add more middlewares to pipeline.
        /// </summary>
        /// <param name="middlewares">Command middlewares.</param>
        public void AddMiddlewares(params ICommandPipelineMiddleware[] middlewares)
        {
            foreach (var middleware in middlewares)
            {
                Middlewares.Add(middleware);
            }
        }

        /// <summary>
        /// Creates default pipeline with command handler locator and executor.
        /// </summary>
        /// <param name="resolver">DI resolver for executor.</param>
        /// <param name="assemblies">Assemblies that contain command handlers.</param>
        /// <returns>Command pipeline.</returns>
        public static CommandPipeline CreateDefaultPipeline(Func<Type, object> resolver, params Assembly[] assemblies)
        {
            var commandPipeline = new CommandPipeline();
            commandPipeline.AddMiddlewares(
                new CommandPipelineMiddlewares.CommandHandlerLocatorMiddleware(assemblies),
                new CommandPipelineMiddlewares.CommandExecutorMiddleware(resolver)
            );
            return commandPipeline;
        }
    }
}
