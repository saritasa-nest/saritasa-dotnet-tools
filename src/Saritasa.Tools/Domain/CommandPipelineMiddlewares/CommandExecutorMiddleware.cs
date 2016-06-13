// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain.CommandPipelineMiddlewares
{
    using System;

    /// <summary>
    /// Default command executor. It does not process commands with Rejected status.
    /// </summary>
    public class CommandExecutorMiddleware : ICommandPipelineMiddleware
    {
        /// <inheritdoc />
        public string Id
        {
            get { return "executor"; }
        }

        Func<Type, object> resolver;

        bool rethrow;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">DI resolver.</param>
        /// <param name="rethrow">Rethrow exceptions that occured during commands execution.</param>
        public CommandExecutorMiddleware(Func<Type, object> resolver, bool rethrow = true)
        {
            this.resolver = resolver;
            this.rethrow = rethrow;
        }

        /// <inheritdoc />
        public void Handle(CommandExecutionContext context)
        {
            if (context.Status == CommandExecutionContext.CommandStatus.Rejected)
            {
                return;
            }

            var handler = resolver(context.HandlerType);
            var parameters = context.HandlerMethod.GetParameters();

            try
            {
                if (parameters.Length == 1)
                {
                    context.HandlerMethod.Invoke(handler, new object[] { context.Command });
                }
                else if (parameters.Length > 1)
                {
                    var paramsarr = new object[parameters.Length];
                    paramsarr[0] = context.Command;
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        paramsarr[i] = resolver(parameters[i].ParameterType);
                    }
                    context.HandlerMethod.Invoke(handler, paramsarr);
                }
                context.Status = CommandExecutionContext.CommandStatus.Completed;
                context.CompletedAt = DateTime.Now;
            }
            catch (Exception)
            {
                context.CompletedAt = DateTime.Now;
                context.Status = CommandExecutionContext.CommandStatus.Failed;
                if (this.rethrow)
                {
                    throw;
                }
            }
        }
    }
}
