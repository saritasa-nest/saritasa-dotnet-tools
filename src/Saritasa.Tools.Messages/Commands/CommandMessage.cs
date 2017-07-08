// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Commands
{
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
        protected internal MethodInfo HandlerMethod { get; set; }

        /// <summary>
        /// Information about the exception source.
        /// </summary>
        protected internal System.Runtime.ExceptionServices.ExceptionDispatchInfo ErrorDispatchInfo { get; set; }

        /// <inheritdoc />
        public override string ErrorMessage => Error?.Message ?? string.Empty;

        /// <inheritdoc />
        public override string ErrorType => Error != null ? Error.GetType().FullName : string.Empty;

        /// <inheritdoc />
        public override string ContentType => Content.GetType().FullName;

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
