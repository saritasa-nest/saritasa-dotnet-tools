// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// The exception occurs when something wrong happens on the server side. When a resource is not available
    /// or works incorrectly so that system cannot process requests. The client must provide source exception.
    /// It can be mapped to 500 HTTP status code.
    /// </summary>
    [Serializable]
    public class InfrastructureException : Exception
    {
        /// <summary>
        /// Optional description code for this exception.
        /// </summary>
        public string Code { get; protected set; } = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public InfrastructureException(Exception innerException) :
            base(DomainErrorDescriber.Default.ServerError(), innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public InfrastructureException(Exception innerException, int code) :
            base(DomainErrorDescriber.Default.ServerError(), innerException)
        {
            this.Code = code.ToString();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public InfrastructureException(Exception innerException, string code) :
            base(DomainErrorDescriber.Default.ServerError(), innerException)
        {
            this.Code = code;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public InfrastructureException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public InfrastructureException(string message, Exception innerException, int code) :
            base(message, innerException)
        {
            this.Code = code.ToString();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public InfrastructureException(string message, Exception innerException, string code) :
            base(message, innerException)
        {
            this.Code = code;
        }

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected InfrastructureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Code = info.GetString("code");
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("code", Code);
        }
    }
}
