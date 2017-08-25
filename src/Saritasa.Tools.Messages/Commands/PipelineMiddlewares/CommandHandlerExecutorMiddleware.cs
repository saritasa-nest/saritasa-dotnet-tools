// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Commands.PipelineMiddlewares
{
    /// <summary>
    /// Default command executor. It does not process commands with Rejected status.
    /// </summary>
    public class CommandHandlerExecutorMiddleware : BaseHandlerExecutorMiddleware
    {
        /// <summary>
        /// Include execution duration.
        /// </summary>
        public bool IncludeExecutionDuration { get; set; } = true;

        /// <summary>
        /// .ctor
        /// </summary>
        public CommandHandlerExecutorMiddleware()
        {
            Id = this.GetType().Name;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Input parameters as parameters.</param>
        public CommandHandlerExecutorMiddleware(IDictionary<string, string> parameters) : base(parameters)
        {
            Id = this.GetType().Name;
        }

        /// <inheritdoc />
        public override void Handle(IMessageContext messageContext)
        {
            var handlerMethod = messageContext.GetItemByKey<MethodInfo>(CommandHandlerLocatorMiddleware.HandlerMethodKey);
            var handler = messageContext.GetItemByKey<object>(CommandHandlerResolverMiddleware.HandlerObjectKey);
            if (handler == null)
            {
                return;
            }

            // Invoke method and resolve parameters if needed.
            Stopwatch stopwatch = null;
            if (IncludeExecutionDuration)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            try
            {
                ExecuteHandler(handler, messageContext.Content, messageContext.ServiceProvider, handlerMethod);
                messageContext.Status = ProcessingStatus.Completed;
            }
            catch (TargetInvocationException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetInvocationException), handler, ex), nameof(CommandHandlerExecutorMiddleware));
                messageContext.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    messageContext.FailException = ex.InnerException;
                }
            }
            catch (TargetException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetException), handler, ex), nameof(CommandHandlerExecutorMiddleware));
                messageContext.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    messageContext.FailException = ex.InnerException;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(Exception), handler, ex), nameof(CommandHandlerExecutorMiddleware));
                messageContext.Status = ProcessingStatus.Failed;
                messageContext.FailException = ex;
            }
            finally
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    messageContext.Items[MessageContextConstants.ExecutionDurationKey] = (int)stopwatch.ElapsedMilliseconds;
                }
            }
        }

        /*
         * There is code duplicate for HandleAsync method. We can make Handle method async instead.
         * But I did this for sake of performance - making method async would add ~9% decrease.
         */

        /// <inheritdoc />
        public override async Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var handlerMethod = messageContext.GetItemByKey<MethodInfo>(CommandHandlerLocatorMiddleware.HandlerMethodKey);
            var handler = messageContext.GetItemByKey<object>(CommandHandlerResolverMiddleware.HandlerObjectKey);
            if (handler == null)
            {
                return;
            }

            // Invoke method and resolve parameters if needed.
            Stopwatch stopwatch = null;
            if (IncludeExecutionDuration)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            try
            {
                await ExecuteHandlerAsync(handler, messageContext.Content, messageContext.ServiceProvider, handlerMethod);
                messageContext.Status = ProcessingStatus.Completed;
            }
            catch (TargetInvocationException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetInvocationException), handler, ex), nameof(CommandHandlerExecutorMiddleware));
                messageContext.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    messageContext.FailException = ex.InnerException;
                }
            }
            catch (TargetException ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(TargetException), handler, ex), nameof(CommandHandlerExecutorMiddleware));
                messageContext.Status = ProcessingStatus.Failed;
                if (ex.InnerException != null)
                {
                    messageContext.FailException = ex.InnerException;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(string.Format(Properties.Strings.ExceptionWhileProcess,
                    nameof(Exception), handler, ex), nameof(CommandHandlerExecutorMiddleware));
                messageContext.Status = ProcessingStatus.Failed;
                messageContext.FailException = ex;
            }
            finally
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    messageContext.Items[MessageContextConstants.ExecutionDurationKey] = (int)stopwatch.ElapsedMilliseconds;
                }
            }
        }
    }
}
