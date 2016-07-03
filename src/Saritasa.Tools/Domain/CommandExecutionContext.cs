// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Command execution context.
    /// </summary>
    public class CommandExecutionContext
    {
        private Guid commandId = Guid.Empty;

        /// <summary>
        /// Commands status.
        /// </summary>
        public enum CommandStatus : byte
        {
            /// <summary>
            /// The command in a processing state.
            /// </summary>
            Processing,

            /// <summary>
            /// Command has been completed.
            /// </summary>
            Completed,

            /// <summary>
            /// Command has been failed while execution. Mostly exception occured
            /// in handler.
            /// </summary>
            Failed,

            /// <summary>
            /// Command has been rejected. It may be validation error.
            /// </summary>
            Rejected,
        }

        /// <summary>
        /// Unique command id.
        /// </summary>
        public Guid CommandId
        {
            get
            {
                if (commandId == Guid.Empty)
                {
                    commandId = Guid.NewGuid();
                }
                return commandId;
            }
        }

        /// <summary>
        /// Command to execute.
        /// </summary>
        public object Command { get; private set; }

        /// <summary>
        /// Command handler.
        /// </summary>
        public Type HandlerType { get; set; }

        /// <summary>
        /// Command handler method to execute.
        /// </summary>
        public MethodInfo HandlerMethod { get; set; }

        /// <summary>
        /// Contains exception if any error occured during command processing.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// When command has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When processing has been completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Processing status.
        /// </summary>
        public CommandStatus Status { get; set; } = CommandStatus.Processing;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="command">Command to execute.</param>
        [System.Diagnostics.DebuggerHidden]
        public CommandExecutionContext(object command)
        {
            Command = command;
            CreatedAt = DateTime.Now;
        }
    }
}
