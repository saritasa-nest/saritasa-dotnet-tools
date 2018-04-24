// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Common.PipelineMiddlewares;

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
        /// Use default middlewares configuration. Includes command handler locator and executor.
        /// </summary>
        /// <param name="optionsAction">Action to configure options.</param>
        /// <returns>Command pipeline builder.</returns>
        public CommandPipelineBuilder Configure(Action<CommandPipelineOptions> optionsAction)
        {
            var options = new CommandPipelineOptions();
            optionsAction(options);

            if (options.UseDefaultPipeline)
            {
                Pipeline.AddMiddlewares(new PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    options.Assemblies.ToArray()));
                Pipeline.AddMiddlewares(new PipelineMiddlewares.CommandHandlerResolverMiddleware());
                Pipeline.AddMiddlewares(new PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    CaptureExceptionDispatchInfo = options.UseExceptionDispatchInfo
                });
                if (options.ThrowExceptionOnFail)
                {
                    Pipeline.AddMiddlewares(new ThrowExceptionOnFailMiddleware
                    {
                        CheckExceptionDispatchInfo = options.UseExceptionDispatchInfo
                    });
                }
            }
            return this;
        }
    }
}
