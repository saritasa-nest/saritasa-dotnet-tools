// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Commands processing pipeline.
    /// </summary>
    public class CommandPipeline
    {
        /// <summary>
        /// Handlers list.
        /// </summary>
        protected IList<ICommandPipelineMiddleware> Handlers { get; set; } = new List<ICommandPipelineMiddleware>();

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        public void Execute(object command)
        {
            var context = new CommandExecutionContext(command);

            foreach (var handler in Handlers)
            {
                handler.Handle(context);
            }
        }

        /// <summary>
        /// Add more handlers to pipeline.
        /// </summary>
        /// <param name="commandHandlers">Command handlers.</param>
        public void AddHandlers(params ICommandPipelineMiddleware[] commandHandlers)
        {
            foreach (var handler in commandHandlers)
            {
                Handlers.Add(handler);
            }
        }
    }
}
