// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Base message context.
    /// </summary>
    public class MessageContext : IMessageContext
    {
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
        public IMessagePipeline Pipeline { get; set; }

        /// <inheritdoc />
        public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageContext()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pipelineService">Pipeline service the context will be related to.</param>
        public MessageContext(IMessagePipelineService pipelineService)
        {
            if (pipelineService == null)
            {
                throw new ArgumentNullException(nameof(pipelineService));
            }
            ServiceProvider = pipelineService.ServiceProvider;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pipelineService">Pipeline service the context will be related to.</param>
        /// <param name="content">Content to process.</param>
        public MessageContext(IMessagePipelineService pipelineService, object content) : this(pipelineService)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            ContentId = TypeHelpers.GetPartiallyAssemblyQualifiedName(content.GetType());
            Content = content;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Id}: {ContentId} [{Status}]";
        }
    }
}
