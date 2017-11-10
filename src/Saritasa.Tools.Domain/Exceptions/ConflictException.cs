// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET40
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// Domain conflict exception. Indicates that the request could not be
    /// processed because of conflict in the request, such as an edit conflict between multiple simultaneous updates.
    /// Can be mapped to 409 HTTP status code.
    /// </summary>
#if NET40
    [Serializable]
#endif
    public class ConflictException : DomainException
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public ConflictException() : base(DomainErrorDescriber.Default.Conflict())
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ConflictException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if NET40
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected ConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
