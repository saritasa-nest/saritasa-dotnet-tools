// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Commands
{
    using Common;

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
        public static ICommandPipeline UseInternalResolver(this ICommandPipeline commandPipeline,
            bool resolveMethodParameters = false)
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
        public static ICommandPipeline UseHandlerSearchMethod(this ICommandPipeline commandPipeline,
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
