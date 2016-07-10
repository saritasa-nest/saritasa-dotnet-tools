// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Command execution result.
    /// </summary>
    public class CommandExecutionResult
    {
        /// <summary>
        /// Unique command id.
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        /// Command name.
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// Command to execute.
        /// </summary>
        public object Command { get; set; }

        /// <summary>
        /// Custom data.
        /// </summary>
        public IDictionary<string, string> Data { get; set; }

        /// <summary>
        /// Contains exception if any error occured during command processing.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// When command has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Command execution duration, in ms.
        /// </summary>
        public long ExecutionDuration { get; set; }

        /// <summary>
        /// Processing status.
        /// </summary>
        public CommandExecutionContext.CommandStatus Status { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public CommandExecutionResult()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="command">Command.</param>
        public CommandExecutionResult(object command)
        {
            Command = command;
        }
    }
}
