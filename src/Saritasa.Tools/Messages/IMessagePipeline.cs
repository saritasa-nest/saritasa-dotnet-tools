// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default message pipeline.
    /// </summary>
    public interface IMessagePipeline
    {
        /// <summary>
        /// Available message types to process.
        /// </summary>
        byte[] MessageTypes { get; }

        /// <summary>
        /// Add more middlewares to pipeline.
        /// </summary>
        /// <param name="middlewares">Middlewares.</param>
        void AddMiddlewares(params IMessagePipelineMiddleware[] middlewares);

        /// <summary>
        /// Gets all middlewares.
        /// </summary>
        /// <returns>Middlewares.</returns>
        IEnumerable<IMessagePipelineMiddleware> GetMiddlewares();

        /// <summary>
        /// Process raw message, for example, from deserialization.
        /// </summary>
        /// <param name="message">Message.</param>
        void ProcessRaw(Message message);
    }
}
