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
        string contentType;

        /// <summary>
        /// Query handler.
        /// </summary>
        public Type HandlerType { get; set; }

        /// <summary>
        /// Query handler method to execute.
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

        /// <summary>
        /// Information about the exception source.
        /// </summary>
        public System.Runtime.ExceptionServices.ExceptionDispatchInfo ErrorDispatchInfo { get; set; }

        /// <inheritdoc />
        public override string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }

        /// <summary>
        /// Execution result.
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Function to execute.
        /// </summary>
        public Delegate Func { get; set; }

        /// <summary>
        /// Function input parameters.
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public QueryMessage()
        {
            Type = Message.MessageTypeQuery;
        }
    }
}
