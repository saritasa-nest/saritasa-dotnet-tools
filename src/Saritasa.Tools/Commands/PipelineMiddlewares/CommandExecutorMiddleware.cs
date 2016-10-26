// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.PipelineMiddlewares
{
    using System;
    using Messages;
    using Internal;

    /// <summary>
    /// Default command executor. It does not process commands with Rejected status.
    /// </summary>
    public class CommandExecutorMiddleware : BaseExecutorMiddleware
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">DI resolver.</param>
        public CommandExecutorMiddleware(Func<Type, object> resolver) : base(resolver)
        {
            Id = "CommandExecutor";
        }

        /// <inheritdoc />
        public override void Handle(Message message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException("Message should be CommandMessage type");
            }

            // rejected commands are not needed to process
            if (commandMessage.Status == Message.ProcessingStatus.Rejected)
            {
                return;
            }

            object handler = null;
            // when command class contains Handle method within
            if (commandMessage.HandlerMethod.DeclaringType == commandMessage.Content.GetType())
            {
                handler = commandMessage.Content;
            }
            else
            {
                handler = ResolveObject(commandMessage.HandlerType, nameof(CommandExecutorMiddleware));
            }

            // if we don't have handler - throw exception
            if (handler == null)
            {
                commandMessage.Status = Message.ProcessingStatus.Rejected;
                throw new CommandHandlerNotFoundException();
            }

            // invoke method and resolve parameters if needed
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                ExecuteHandler(handler, commandMessage.Content, commandMessage.HandlerMethod);
                commandMessage.Status = Message.ProcessingStatus.Completed;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn($"Exception while process \"{handler}\": {ex}", nameof(CommandExecutorMiddleware));
                commandMessage.Status = Message.ProcessingStatus.Failed;
                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    commandMessage.Error = innerException;
                    commandMessage.ErrorDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(innerException);
                }
                else
                {
                    InternalLogger.Warn($"For some reason InnerException is null. Type: {ex.GetType()}.", nameof(CommandExecutorMiddleware));
                }
            }
            finally
            {
                stopWatch.Stop();
                commandMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
            }
        }
    }
}
