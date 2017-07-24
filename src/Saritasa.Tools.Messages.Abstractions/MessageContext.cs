// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Base message context.
    /// </summary>
    public class MessageContext : IMessageContext
    {
        /// <summary>
        /// Specifies key to be used in items to determine what pipeline
        /// should be used to process message.
        /// </summary>
        public const string TypeKey = ".type";

        /// <summary>
        /// Specifies key to be sued in items to get user key/value
        /// dictionary with additional processing data. For key/value use <see cref="string" /> type.
        /// </summary>
        public const string DataKey = ".data";

        /// <summary>
        /// Processing execution duration in milliseconds.
        /// </summary>
        public const string ExecutionDurationKey = ".execution-duration";

        private Guid id = Guid.Empty;

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

            set => id = value;
        }

        /// <inheritdoc />
        public string ContentId { get; set; }

        /// <inheritdoc />
        public object Content { get; set; }

        /// <inheritdoc />
        public ProcessingStatus Status { get; set; } = ProcessingStatus.NotInitialized;

        /// <inheritdoc />
        public Exception FailException { get; set; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; set; } = NullServiceProvider.Default;

        /// <inheritdoc />
        public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

        /// <summary>
        /// .ctor
        /// </summary>
        public MessageContext()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="pipelineService">Pipeline service the context will be related to.</param>
        public MessageContext(IPipelineService pipelineService)
        {
            if (pipelineService == null)
            {
                throw new ArgumentNullException(nameof(pipelineService));
            }
            ServiceProvider = pipelineService.ServiceProvider;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="pipelineService">Pipeline service the context will be related to.</param>
        /// <param name="content">Content to process.</param>
        public MessageContext(IPipelineService pipelineService, object content) : this(pipelineService)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            ContentId = content.GetType().FullName;
            Content = content;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Id}: {ContentId} [{Status}]";
        }
    }
}
