// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message context.
    /// </summary>
    public interface IMessageContext
    {
        /// <summary>
        /// Unique message id.
        /// </summary>
        Guid Id { get; }

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
        Exception FailedException { get; set; }

        /// <summary>
        /// Current used service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Local key/value collection of objects that are shared across current message scope.
        /// </summary>
        IDictionary<object, object> Items { get; set; }

        /// <summary>
        /// Global key/value collection of objects that are shared globally between every
        /// message context creation.
        /// </summary>
        IDictionary<object, object> GlobalItems { get; set; }
    }
}
