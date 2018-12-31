// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message processing exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MessageProcessingException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageProcessingException() : base("Message processing error.")
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageProcessingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if NET452
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected MessageProcessingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
