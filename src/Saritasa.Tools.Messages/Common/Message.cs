// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Abstractions;

    /// <summary>
    /// Message execution context.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class Message : IMessage
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
        ,ICloneable
#endif
    {
        /// <summary>
        /// Command type.
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

        internal const int MessageFieldIdIndex = 0;
        internal const int MessageFieldTypeIndex = 1;
        internal const int MessageFieldContentTypeIndex = 2;
        internal const int MessageFieldContentIndex = 3;
        internal const int MessageFieldDataIndex = 4;
        internal const int MessageFieldErrorDetailsIndex = 5;
        internal const int MessageFieldErrorMessageIndex = 6;
        internal const int MessageFieldErrorTypeIndex = 7;
        internal const int MessageFieldCreatedAtIndex = 8;
        internal const int MessageFieldExecutionDurationIndex = 9;
        internal const int MessageFieldStatusIndex = 10;

        IDictionary<string, string> data;

        Guid id;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public byte Type { get; set; }

        /// <inheritdoc />
        public virtual string ContentType { get; set; }

        /// <inheritdoc />
        public object Content { get; set; }

        /// <inheritdoc />
        public virtual IDictionary<string, string> Data
        {
            get { return data ?? (data = new Dictionary<string, string>()); }

            set
            {
                data = value;
            }
        }

        /// <inheritdoc />
        public virtual Exception Error { get; set; }

        /// <inheritdoc />
        public virtual string ErrorMessage { get; set; }

        /// <inheritdoc />
        public virtual string ErrorType { get; set; }

        /// <inheritdoc />
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc />
        public int ExecutionDuration { get; set; }

        /// <inheritdoc />
        public ProcessingStatus Status { get; set; }

        /// <summary>
        /// Is custom data dictionary initialized.
        /// </summary>
        [JsonIgnore]
        public bool HasData => data != null;

        /// <summary>
        /// Does message contain error.
        /// </summary>
        [JsonIgnore]
        public bool HasError => Error != null;

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

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Returns cloned object with only <inheritdoc cref="Message" /> properties.
        /// </summary>
        /// <returns>Cloned object.</returns>
        public Message CloneToMessage()
        {
            var message = new Message()
            {
                Content = this.Content,
                ContentType = this.ContentType,
                CreatedAt = this.CreatedAt,
                Error = this.Error,
                ErrorMessage = this.ErrorMessage,
                ErrorType = this.ErrorType,
                ExecutionDuration = this.ExecutionDuration,
                Id = this.id,
                Status = this.Status,
                Type = this.Type
            };
            if (HasData)
            {
                foreach (var item in data)
                {
                    message.Data[item.Key] = item.Value;
                }
            }
            return message;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Id}: {ContentType} [{Status}]";
        }
    }
}
