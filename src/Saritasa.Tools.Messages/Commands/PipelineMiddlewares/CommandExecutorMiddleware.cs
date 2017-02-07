// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Commands.PipelineMiddlewares
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Abstractions;
    using Common;
    using Internal;

    /// <summary>
    /// Default command executor. It does not process commands with Rejected status.
    /// </summary>
    public class CommandExecutorMiddleware : BaseExecutorMiddleware
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">Dependency injection resolver.</param>
        public CommandExecutorMiddleware(Func<Type, object> resolver) : base(resolver)
        {
            Id = "CommandExecutor";
        }

#if !NET40 && !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private object GetHandler(CommandMessage commandMessage)
        {
            // Rejected commands are not needed to process.
            if (commandMessage.Status == ProcessingStatus.Rejected)
            {
                return null;
            }

            object handler = null;

            // When command class contains Handle method within.
            if (commandMessage.HandlerMethod.DeclaringType == commandMessage.Content.GetType())
            {
                handler = commandMessage.Content;
            }
            else
            {
                handler = ResolveObject(commandMessage.HandlerType, nameof(CommandExecutorMiddleware));
            }

            // If we don't have handler - throw exception.
            if (handler == null)
            {
                commandMessage.Status = ProcessingStatus.Rejected;
                throw new CommandHandlerNotFoundException(commandMessage.Content.GetType().Name);
            }
            return handler;
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

            var handler = GetHandler(commandMessage);
            if (handler == null)
            {
                return;
            }

            // Invoke method and resolve parameters if needed.
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                ExecuteHandler(handler, commandMessage.Content, commandMessage.HandlerMethod);
                commandMessage.Status = ProcessingStatus.Completed;
            }
            catch (TargetInvocationException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetInvocationException), handler, ex), nameof(CommandExecutorMiddleware));
                commandMessage.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    commandMessage.Error = ex.InnerException;
                    commandMessage.ErrorDispatchInfo =
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException);
                }
            }
            catch (TargetException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetException), handler, ex), nameof(CommandExecutorMiddleware));
                commandMessage.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    commandMessage.Error = ex.InnerException;
                    commandMessage.ErrorDispatchInfo =
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(Exception), handler, ex), nameof(CommandExecutorMiddleware));
                commandMessage.Status = ProcessingStatus.Failed;
                commandMessage.Error = ex;
                commandMessage.ErrorDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                // Release handler.
                var disposable = handler as IDisposable;
                disposable?.Dispose();

                stopWatch.Stop();
                commandMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
            }
        }

        /*
         * There is code duplicate for HandleAsync method. We can make Handle method async instead.
         * But I did this for sake of performance - making method async would add ~9% decrease.
         */

        /// <inheritdoc />
        public override async Task HandleAsync(IMessage message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(CommandMessage)));
            }

            var handler = GetHandler(commandMessage);
            if (handler == null)
            {
                return;
            }

            // Invoke method and resolve parameters if needed.
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await ExecuteHandlerAsync(handler, commandMessage.Content, commandMessage.HandlerMethod);
                commandMessage.Status = ProcessingStatus.Completed;
            }
            catch (TargetInvocationException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetInvocationException), handler, ex), nameof(CommandExecutorMiddleware));
                commandMessage.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    commandMessage.Error = ex.InnerException;
                    commandMessage.ErrorDispatchInfo =
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException);
                }
            }
            catch (TargetException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetException), handler, ex), nameof(CommandExecutorMiddleware));
                commandMessage.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    commandMessage.Error = ex.InnerException;
                    commandMessage.ErrorDispatchInfo =
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(Exception), handler, ex), nameof(CommandExecutorMiddleware));
                commandMessage.Status = ProcessingStatus.Failed;
                commandMessage.Error = ex;
                commandMessage.ErrorDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                stopWatch.Stop();
                commandMessage.ExecutionDuration = (int)stopWatch.ElapsedMilliseconds;
            }
        }
    }
}
