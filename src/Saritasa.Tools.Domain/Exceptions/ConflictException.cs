// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// Domain conflict exception. Indicates that the request could not be
    /// processed because of conflict in the request, such as an edit conflict between multiple simultaneous updates.
    /// Can be mapped to 409 HTTP status code.
    /// </summary>
    [Serializable]
    public class ConflictException : DomainException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConflictException() : base(DomainErrorDescriber.Default.Conflict())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="code">Optional description code for this exception.</param>
        public ConflictException(int code) : base(DomainErrorDescriber.Default.Conflict(), code)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ConflictException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public ConflictException(string message, int code) : base(message, code)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public ConflictException(string message, string code) : base(message, code)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public ConflictException(string message, Exception innerException, int code) :
            base(message, innerException, code)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public ConflictException(string message, Exception innerException, string code) :
            base(message, innerException, code)
        {
        }

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected ConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
