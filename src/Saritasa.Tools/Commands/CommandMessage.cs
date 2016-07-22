// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
{
    using Messages;
    using System;
    using System.Reflection;

    /// <summary>
    /// Command execution context.
    /// </summary>
    public class CommandMessage : Message
    {
        /// <summary>
        /// Command handler.
        /// </summary>
        public Type HandlerType { get; set; }

        /// <summary>
        /// Command handler method to execute.
        /// </summary>
        public MethodInfo HandlerMethod { get; set; }

        /// <inheritdoc />
        public override string ErrorMessage
        {
            get
            {
                return ErrorDetails != null ? ErrorDetails.Message : string.Empty;
            }
        }

        /// <inheritdoc />
        public override string ErrorType
        {
            get
            {
                return ErrorDetails != null ? ErrorDetails.GetType().FullName : string.Empty;
            }
        }

        /// <inheritdoc />
        public override string ContentType
        {
            get
            {
                return Content.GetType().FullName;
            }
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public CommandMessage()
        {
            Type = Message.MessageTypeCommand;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="command">Command message.</param>
        public CommandMessage(object command) : base(command, Message.MessageTypeCommand)
        {
        }
    }
}
