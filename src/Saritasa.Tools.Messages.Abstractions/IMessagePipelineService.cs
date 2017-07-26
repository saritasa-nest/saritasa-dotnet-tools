// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Pipelines service.
    /// </summary>
    public interface IMessagePipelineService
    {
        /// <summary>
        /// Pipelines related to service.
        /// </summary>
        IMessagePipelineContainer PipelineContainer { get; set; }

        /// <summary>
        /// Current used service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }
    }
}
