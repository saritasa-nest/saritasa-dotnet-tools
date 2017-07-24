// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Container to store message pipelines.
    /// </summary>
    public interface IMessagePipelineContainer
    {
        /// <summary>
        /// Pipelines array.
        /// </summary>
        IMessagePipeline[] Pipelines { get; set; }
    }
}
