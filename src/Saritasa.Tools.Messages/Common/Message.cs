// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Command execution result.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Comamnd type.
        /// </summary>
        public const byte MessageTypeCommand = 1;

        /// <summary>
        /// Query type.
        /// </summary>
        public const byte MessageTypeQuery = 2;

        /// <summary>
        /// Event type.
        /// </summary>
        public const byte MessageTypeEvent = 3;

        internal const int MessageFieldIdInd = 0;
        internal const int MessageFieldTypeInd = 1;
        internal const int MessageFieldContentTypeInd = 2;
        internal const int MessageFieldContentInd = 3;
        internal const int MessageFieldDataInd = 4;
        internal const int MessageFieldErrorDetailsInd = 5;
        internal const int MessageFieldErrorMessageInd = 6;
        internal const int MessageFieldErrorTypeInd = 7;
        internal const int MessageFieldCreatedAtInd = 8;
        internal const int MessageFieldExecutionDurationInd = 9;
        internal const int MessageFieldStatusInd = 10;

        /// <summary>
        /// Commands status.
        /// </summary>
        public enum ProcessingStatus : byte
        {
            /// <summary>
            /// Default command state.
            /// </summary>
            NotInitialized,

            /// <summary>
            /// The command in a processing state.
            /// </summary>
            Processing,

            /// <summary>
            /// Command has been completed.
            /// </summary>
            Completed,

            /// <summary>
            /// Command has been failed while execution. Mostly exception occured
            /// in handler.
            /// </summary>
            Failed,

            /// <summary>
            /// Command has been rejected. It may be validation error.
            /// </summary>
            Rejected,
        }

        IDictionary<string, string> data;

        Guid id;

        /// <summary>
        /// Unique message id.
        /// </summary>
        public virtual Guid Id
        {
            get
            {
                if (id == Guid.Empty)
                {
                    id = Guid.NewGuid();
                }
                return id;
            }

            set
            {
                id = value;
            }
        }

        /// <summary>
        /// Message type.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// Message name.
        /// </summary>
        public virtual string ContentType { get; set; }

        /// <summary>
        /// Message content. May be command object, or event object.
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Custom data.
        /// </summary>
        public virtual IDictionary<string, string> Data
        {
            get { return data ?? (data = new Dictionary<string, string>()); }

            set
            {
                data = value;
            }
        }

        /// <summary>
        /// Contains exception if any error occured during message processing.
        /// </summary>
        public virtual Exception Error { get; set; }

        /// <summary>
        /// Error text message.
        /// </summary>
        public virtual string ErrorMessage { get; set; }

        /// <summary>
        /// Error type.
        /// </summary>
        public virtual string ErrorType { get; set; }

        /// <summary>
        /// When message has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Message execution duration, in ms.
        /// </summary>
        public int ExecutionDuration { get; set; }

        /// <summary>
        /// Processing status.
        /// </summary>
        public ProcessingStatus Status { get; set; }

        /// <summary>
        /// Is custom data dictionary initialized.
        /// </summary>
        [JsonIgnore]
        public bool HasData => data != null;

        /// <summary>
        /// .ctor
        /// </summary>
        public Message()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="type">Type.</param>
        public Message(object message, byte type)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            Content = message;
            Type = type;
            CreatedAt = DateTime.Now;
        }
    }
}
