// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET40 || NET452
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// Exception occurs when something wrong happens on server side. When a resource is not available
    /// or works incorrect so that system cannot process request. The client must provide source exception.
    /// Can be mapped to 500 HTTP status code.
    /// </summary>
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

#if NET40 || NET452
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
#endif
    }
}
