// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    public class CommandPipelineBuilder : BasePipelineBuilder<ICommandPipeline>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="commandPipeline">Command pipeline.</param>
        public CommandPipelineBuilder(ICommandPipeline commandPipeline) : base(commandPipeline)
        {
        }

        /// <summary>
        /// Add middleware to command pipeline.
        /// </summary>
        /// <param name="middleware">Middleware to add.</param>
        /// <returns>Command pipeline builder.</returns>
        public CommandPipelineBuilder AddMiddleware(IMessagePipelineMiddleware middleware)
        {
            Pipeline.AddMiddlewares(middleware);
            return this;
        }

        /// <summary>
        /// Use default middlewares configuration.
        /// </summary>
        public CommandPipelineBuilder AddStandardMiddlewares()
        {
            return AddStandardMiddlewares(options => { });
        }

        /// <summary>
        /// Use default middlewares configuration. Includes command handler locator and executor.
        /// </summary>
        /// <param name="optionsAction">Action to configure options.</param>
        /// <returns>Command pipeline builder.</returns>
        public CommandPipelineBuilder AddStandardMiddlewares(Action<CommandPipelineOptions> optionsAction)
        {
            var options = new CommandPipelineOptions();
            optionsAction(options);

            Pipeline.AddMiddlewares(new Common.PipelineMiddlewares.PrepareMessageContextMiddleware());
            Pipeline.AddMiddlewares(new PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                options.GetAssemblies()));
            Pipeline.AddMiddlewares(new PipelineMiddlewares.CommandHandlerResolverMiddleware(
                options.InternalResolver.UseInternalObjectResolver)
            {
                UsePropertiesResolving = options.InternalResolver.UsePropertiesResolving
            });
            Pipeline.AddMiddlewares(new PipelineMiddlewares.CommandHandlerExecutorMiddleware(options.ThrowExceptionOnFail)
            {
                CaptureExceptionDispatchInfo = options.UseExceptionDispatchInfo,
                UseParametersResolve = options.InternalResolver.UseHandlerParametersResolve
            });
            return this;
        }
    }
}
