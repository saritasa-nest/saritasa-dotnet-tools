// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages
{
    /// <summary>
    /// Endpoint to enter messages to system.
    /// </summary>
    public interface IMessageEndpoint
    {
        /// <summary>
        /// Register pipelines to accept incoming messages.
        /// </summary>
        /// <param name="pipelines">Pipelines.</param>
        void RegisterPipelines(params IMessagePipeline[] pipelines);
    }
}
