// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
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
        public static ICommandPipeline UseInternalResolver(this ICommandPipeline commandPipeline,
            bool resolveMethodParameters = false)
        {
            var middleware = (CommandPipelineMiddlewares.CommandExecutorMiddleware)commandPipeline.GetMiddlewareById("CommandExecutor");
            middleware.UseInternalObjectResolver = true;
            middleware.UseParametersResolve = resolveMethodParameters;
            return commandPipeline;
        }
    }
}
