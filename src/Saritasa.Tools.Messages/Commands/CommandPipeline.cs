// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Commands specific pipeline.
    /// </summary>
    public class CommandPipeline : MessagePipeline, ICommandPipeline
    {
        static readonly byte[] availableMessageTypes = { Message.MessageTypeCommand };

        /// <inheritdoc />
        public override byte[] MessageTypes => availableMessageTypes;

        #region ICommandPipeline

        /// <inheritdoc />
        public virtual void Handle(object command)
        {
            var commandMessage = new CommandMessage(command);
            ProcessMiddlewares(commandMessage);
            commandMessage.ErrorDispatchInfo?.Throw();
        }

        /// <inheritdoc />
        public virtual async Task HandleAsync(object command, CancellationToken cancellationToken)
        {
            var commandMessage = new CommandMessage(command);
            await ProcessMiddlewaresAsync(commandMessage, cancellationToken);
            commandMessage.ErrorDispatchInfo?.Throw();
        }

        #endregion

        /// <summary>
        /// Resolver that always returns null values.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <returns>Null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "type",
            Justification = "Mock method")]
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
                assemblies = new[] { Assembly.GetEntryAssembly() };
            }

            commandPipeline.AppendMiddlewares(
                new PipelineMiddlewares.CommandHandlerLocatorMiddleware(assemblies),
                new PipelineMiddlewares.CommandExecutorMiddleware(resolver)
            );
            return commandPipeline;
        }

        /// <inheritdoc />
        public override void ProcessRaw(IMessage message)
        {
            var commandMessage = new CommandMessage(message.Content);
            ProcessMiddlewares(commandMessage);
            message.Content = commandMessage.Content;
            message.Error = commandMessage.Error;
            message.ErrorMessage = commandMessage.ErrorMessage;
            message.ErrorType = commandMessage.ErrorType;
            message.Status = commandMessage.Status;
            message.ExecutionDuration = commandMessage.ExecutionDuration;
            message.Data = commandMessage.Data;
        }
    }
}
