// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Command pipeline extensions.
    /// </summary>
    public static class PipelinesServiceExtensions
    {
        /// <summary>
        /// Add command pipeline feature to message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <returns>Command pipeline builder.</returns>
        public static CommandPipelineBuilder AddCommandPipeline(this IPipelinesService pipelinesService)
        {
            return AddCommandPipeline(pipelinesService, options => { });
        }

        /// <summary>
        /// Add command pipeline feature to message context.
        /// </summary>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <param name="setupAction">Action to setup command pipeline.</param>
        /// <returns>Command pipeline builder.</returns>
        public static CommandPipelineBuilder AddCommandPipeline(this IPipelinesService pipelinesService,
            Action<CommandPipelineOptions> setupAction)
        {
            if (pipelinesService.Pipelines.Any(p => p is ICommandPipeline))
            {
                throw new InvalidOperationException("Command pipeline already exists in global context items. " +
                    "Use RemovePipeline method to clean up existins pipeline.");
            }

            var commandPipeline = new CommandPipeline();
            setupAction(commandPipeline.Options);
            var list = pipelinesService.Pipelines.ToList();
            list.Add(commandPipeline);
            pipelinesService.Pipelines = list.ToArray();

            return new CommandPipelineBuilder(commandPipeline);
        }
    }
}
