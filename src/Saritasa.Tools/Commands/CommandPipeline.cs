// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
{
    using System;
    using System.Reflection;
    using Messages;

    /// <summary>
    /// Commands specific pipeline.
    /// </summary>
    public class CommandPipeline : MessagePipeline, ICommandPipeline
    {
        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        [System.Diagnostics.DebuggerHidden]
        public void Handle(object command)
        {
            var commandMessage = new CommandMessage(command);

            foreach (var handler in Middlewares)
            {
                handler.Handle(commandMessage);
            }

            if (commandMessage.ErrorDispatchInfo != null)
            {
                commandMessage.ErrorDispatchInfo.Throw();
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
