// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
{
    using System;

    /// <summary>
    /// Pipeline handler to process command. It may change command exectuion context.
    /// </summary>
    public interface ICommandPipelineMiddleware
    {
        /// <summary>
        /// Custom id field of handler. Just to identify it for debugging purposes.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Executes the command in context.
        /// </summary>
        /// <param name="context">Command execution context.</param>
        void Execute(CommandExecutionContext context);
    }
}
