// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Append additional data to command context.
    /// </summary>
    public class CommandDataMiddleware : ICommandPipelineMiddleware
    {
        /// <inheritdoc />
        public string Id
        {
            get { return "delegate"; }
        }

        Action<IDictionary<string, string>> action;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public CommandDataMiddleware(Action<IDictionary<string, string>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            this.action = action;
        }

        /// <inheritdoc />
        public void Execute(CommandExecutionContext context)
        {
            action(context.Data);
        }
    }
}
