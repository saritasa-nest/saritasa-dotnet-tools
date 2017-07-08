// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// Domain forbidden security exception. Can be mapped to 403 HTTP status code.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class ForbiddenException : DomainException
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public ForbiddenException() : base(DomainErrorDescriber.Default.Forbidden())
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ForbiddenException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ForbiddenException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected ForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        /// <summary>
        /// Shortcut to throw exception if certain condition is false. Does not require to
        /// write if block.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Exception message.</param>
        public static void ThrowIfFalse(bool condition, string message)
        {
            if (condition == false)
            {
                throw new ForbiddenException(message);
            }
        }

        /// <summary>
        /// Shortcut to throw exception if certain condition is false. Does not require to
        /// write if block.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public static void ThrowIfFalse(bool condition, string message, Exception innerException)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException(nameof(innerException));
            }
            if (condition == false)
            {
                throw new ForbiddenException(message, innerException);
            }
        }

        /// <summary>
        /// Shortcut to throw exception if certain condition is true. Does not require to
        /// write if block.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Exception message.</param>
        public static void ThrowIfTrue(bool condition, string message)
        {
            if (condition)
            {
                throw new ForbiddenException(message);
            }
        }

        /// <summary>
        /// Shortcut to throw exception if certain condition is true. Does not require to
        /// write if block.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public static void ThrowIfTrue(bool condition, string message, Exception innerException)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException(nameof(innerException));
            }
            if (condition)
            {
                throw new ForbiddenException(message, innerException);
            }
        }
    }
}
