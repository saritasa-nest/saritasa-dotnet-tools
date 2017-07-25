// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Messages.Configuration
{
    /// <summary>
    /// Saritasa Tools messages configuration exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MessagesConfigurationException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public MessagesConfigurationException()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public MessagesConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public MessagesConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if NET452
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected MessagesConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
