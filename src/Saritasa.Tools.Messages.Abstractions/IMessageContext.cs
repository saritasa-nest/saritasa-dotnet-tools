// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message context. Contains objects to execute, metadata and current execution state.
    /// The specific instance is created for every request.
    /// </summary>
    public interface IMessageContext
    {
        /// <summary>
        /// Unique message id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Current service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Identifier for content. Can be used to determine unique
        /// processing content type and where to find base class.
        /// </summary>
        string ContentId { get; set; }

        /// <summary>
        /// Current message content. May be command object or event object.
        /// </summary>
        object Content { get; set; }

        /// <summary>
        /// Get or sets current status of message context execution.
        /// </summary>
        ProcessingStatus Status { get; set; }

        /// <summary>
        /// Exception that describes error when status is failed.
        /// </summary>
        Exception FailException { get; set; }

        /// <summary>
        /// Current processing pipeline or null.
        /// </summary>
        IMessagePipeline Pipeline { get; set; }

        /// <summary>
        /// Local key/value collection of objects that are shared across current message scope.
        /// Expect that dictionary implementation can not be thread safe.
        /// </summary>
        IDictionary<object, object> Items { get; set; }
    }
}
