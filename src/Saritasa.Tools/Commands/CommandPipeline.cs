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
        static readonly byte[] AvailableMessageTypes = new byte[] { Message.MessageTypeCommand };

        /// <inheritdoc />
        public override byte[] MessageTypes => AvailableMessageTypes;

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
        /// Resolver that always returns null values.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <returns>Null.</returns>
        public static object NullResolver(Type type)
        {
            return null;
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
            if (assemblies == null || assemblies.Length < 1)
            {
                assemblies = new Assembly[] { Assembly.GetEntryAssembly() };
            }

            commandPipeline.AddMiddlewares(
                new CommandPipelineMiddlewares.CommandHandlerLocatorMiddleware(assemblies),
                new CommandPipelineMiddlewares.CommandExecutorMiddleware(resolver)
            );
            return commandPipeline;
        }

        /// <inheritdoc />
        public override void ProcessRaw(Message message)
        {
            Handle(message.Content);
        }
    }
}
