// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Messages;

    /// <summary>
    /// Event execution context.
    /// </summary>
    public class EventMessage : Message
    {
        /// <summary>
        /// Event handler methods to execute.
        /// </summary>
        protected internal List<MethodInfo> HandlerMethods { get; set; }

        /// <inheritdoc />
        public override string ErrorMessage => Error?.Message ?? string.Empty;

        /// <inheritdoc />
        public override string ErrorType => Error != null ? Error.GetType().FullName : string.Empty;

        /// <inheritdoc />
        public override string ContentType => Content.GetType().FullName;

        /// <summary>
        /// Function input parameters.
        /// </summary>
        protected internal object[] Parameters { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public EventMessage()
        {
            Type = Message.MessageTypeEvent;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="event">Events message.</param>
        public EventMessage(object @event) : base(@event, Message.MessageTypeEvent)
        {
        }
    }
}
