// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
#if NET452
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message data transfer object used by repositories to save/load messages.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MessageRecord
    {
        /// <summary>
        /// Unique message id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Message type.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// Message type name.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Message serialized content. May be command object, or event object.
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Custom data. Should be <see cref="IDictionary{TKey,TValue}"/> where
        /// TKey and TValue are <see cref="string" /> type.
        /// </summary>
        public IDictionary<string, string> Data { get; set; }

        /// <summary>
        /// Contains exception if any error occurred during message processing.
        /// </summary>
        public object Error { get; set; }

        /// <summary>
        /// Error text message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error type.
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// When message has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Message execution duration, in ms.
        /// </summary>
        public int ExecutionDuration { get; set; }

        /// <summary>
        /// Processing status.
        /// </summary>
        public ProcessingStatus Status { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MessageRecord()
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Id}-{ContentType}";
        }

#if NET452
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected MessageRecord(SerializationInfo info, StreamingContext context)
        {
        }
#endif
    }
}
