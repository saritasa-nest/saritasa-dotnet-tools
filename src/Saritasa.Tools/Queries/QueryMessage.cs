// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Queries
{
    using Messages;
    using System;
    using System.Reflection;

    /// <summary>
    /// Query execution context.
    /// </summary>
    public class QueryMessage : Message
    {
        /// <summary>
        /// Query handler.
        /// </summary>
        protected internal Type HandlerType { get; set; }

        /// <summary>
        /// Query handler method to execute.
        /// </summary>
        protected internal MethodInfo HandlerMethod { get; set; }

        /// <inheritdoc />
        public override string ErrorMessage => Error?.Message ?? string.Empty;

        /// <inheritdoc />
        public override string ErrorType => Error != null ? Error.GetType().FullName : string.Empty;

        /// <summary>
        /// Information about the exception source.
        /// </summary>
        public System.Runtime.ExceptionServices.ExceptionDispatchInfo ErrorDispatchInfo { get; set; }

        /// <inheritdoc />
        public override string ContentType { get; set; }

        /// <summary>
        /// Execution result.
        /// </summary>
        protected internal object Result { get; set; }

        /// <summary>
        /// Function to execute.
        /// </summary>
        protected internal Delegate Func { get; set; }

        /// <summary>
        /// Query object the delegate will be executed against.
        /// </summary>
        protected internal object QueryObject { get; set; }

        /// <summary>
        /// Function input parameters.
        /// </summary>
        protected internal object[] Parameters { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public QueryMessage()
        {
            Type = Message.MessageTypeQuery;
        }
    }
}
