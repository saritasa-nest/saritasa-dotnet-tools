// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain.Exceptions
{
    using System;
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    using System.Runtime.Serialization;
#endif

    /// <summary>
    /// Exception occurs in domain part of application. It can be logic or validation exception.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class DomainException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public DomainException() : base("Error")
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public DomainException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
