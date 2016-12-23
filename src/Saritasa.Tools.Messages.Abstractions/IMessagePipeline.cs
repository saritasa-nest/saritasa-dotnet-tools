// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Abstractions
{
    using System;
    using System.Collections.Generic;

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
        void AppendMiddlewares(params IMessagePipelineMiddleware[] middlewares);

        /// <summary>
        /// Gets all middlewares.
        /// </summary>
        /// <returns>Middlewares.</returns>
        IEnumerable<IMessagePipelineMiddleware> GetMiddlewares();

        /// <summary>
        /// Get middle by its id. If not found null will be returned.
        /// </summary>
        /// <param name="id">Middleware id.</param>
        /// <returns>Middleware or null if not found.</returns>
        IMessagePipelineMiddleware GetMiddlewareById(string id);

        /// <summary>
        /// Insert middleware after specified one. If not specified it will be inserted as last.
        /// </summary>
        /// <param name="middleware">Middleware to insert.</param>
        /// <param name="insertAfterId">Insert after middleware with specified id. If null will be inserted
        /// as last.</param>
        void InsertMiddlewareAfter(IMessagePipelineMiddleware middleware, string insertAfterId = null);

        /// <summary>
        /// Insert middleware before specified one. If not specified it will be inserted as first.
        /// </summary>
        /// <param name="middleware">Middleware to insert.</param>
        /// <param name="insertBeforeId">Insert before middleware with specified id. If null will be inserted
        /// as first.</param>
        void InsertMiddlewareBefore(IMessagePipelineMiddleware middleware, string insertBeforeId);

        /// <summary>
        /// Remove middleware by id.
        /// </summary>
        /// <param name="id">Middleware id.</param>
        void RemoveMiddleware(string id);

        /// <summary>
        /// Process raw message, for example, from deserialization.
        /// </summary>
        /// <param name="message">Message.</param>
        void ProcessRaw(IMessage message);
    }
}
