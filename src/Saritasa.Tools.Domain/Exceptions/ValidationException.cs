// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain.Exceptions
{
    using System;
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    using System.Runtime.Serialization;
#endif

    /// <summary>
    /// Validation exception.
    /// </summary>
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class ValidationException : DomainException
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public ValidationException()
        {
        }

        /// <summary>
        /// .ctor with message.
        /// <param name="message">Exception message.</param>
        /// </summary>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor with message and inner exception.
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        /// </summary>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
