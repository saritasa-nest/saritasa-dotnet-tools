// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Pipelines service.
    /// </summary>
    public interface IPipelineService
    {
        /// <summary>
        /// Pipelines related to service.
        /// </summary>
        IMessagePipelineContainer PipelineContainer { get; set; }

        /// <summary>
        /// Current used service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Local key/value collection of objects that are shared across current message scope.
        /// Expect that dictionary implementation can not be thread safe.
        /// </summary>
        IDictionary<object, object> Items { get; set; }
    }
}
