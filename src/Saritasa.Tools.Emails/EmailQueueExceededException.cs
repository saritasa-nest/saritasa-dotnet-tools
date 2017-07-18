// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Emails
{
    /// <summary>
    /// Exception occurs when email sending queue is overloaded.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class EmailQueueExceededException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="maxSize">Max queue size that has been exceeded.</param>
        public EmailQueueExceededException(int maxSize) : base(
            string.Format(Properties.Strings.EmailQueueSizeExceed, maxSize.ToString()))
        {
        }

#if NET452
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected EmailQueueExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
