// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// The exception occurs on error while parse query text.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MessageQueryParseException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageQueryParseException() : base()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public MessageQueryParseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public MessageQueryParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if NET452
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected MessageQueryParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
