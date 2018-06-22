// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Endpoint to enter messages to system.
    /// </summary>
    public interface IMessageEndpoint
    {
        /// <summary>
        /// Register pipelines to accept incoming messages.
        /// </summary>
        /// <param name="container">Pipelines container.</param>
        void RegisterPipelines(IMessagePipelineContainer container);
    }
}
