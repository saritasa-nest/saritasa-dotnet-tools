// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using JetBrains.Annotations;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Command pipeline extensions.
    /// </summary>
    public static class CommandPipelineExtensions
    {
        /// <summary>
        /// Use internal IoC container.
        /// </summary>
        /// <param name="commandPipeline">Command pipeline.</param>
        /// <param name="resolveMethodParameters">Resolve method parameters.</param>
        /// <returns>Command pipeline.</returns>
        public static ICommandPipeline UseInternalResolver(
            [NotNull] this ICommandPipeline commandPipeline,
            bool resolveMethodParameters = true)
        {
            var middleware = (PipelineMiddlewares.CommandExecutorMiddleware)commandPipeline.GetMiddlewareById("CommandExecutor");
            if (middleware == null)
            {
                throw new MiddlewareNotFoundException();
            }
            middleware.UseInternalObjectResolver = true;
            middleware.UseParametersResolve = resolveMethodParameters;
            return commandPipeline;
        }

        /// <summary>
        /// Use another method to search handlers.
        /// </summary>
        /// <param name="commandPipeline">Command pipeline.</param>
        /// <param name="searchMethod">Handlers search method.</param>
        /// <returns>Command pipeline.</returns>
        public static ICommandPipeline UseHandlerSearchMethod(
            [NotNull] this ICommandPipeline commandPipeline,
            HandlerSearchMethod searchMethod)
        {
            var middleware = (PipelineMiddlewares.CommandHandlerLocatorMiddleware)commandPipeline.GetMiddlewareById("CommandHandlerLocator");
            if (middleware == null)
            {
                throw new MiddlewareNotFoundException();
            }
            middleware.HandlerSearchMethod = searchMethod;
            return commandPipeline;
        }
    }
}
