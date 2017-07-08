// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Queries specific pipeline.
    /// </summary>
    public interface IQueryPipeline : IMessagePipeline
    {
        /// <summary>
        /// Creates caller wrapper.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <returns>Query wrapper.</returns>
        ICaller<TQuery> Query<TQuery>() where TQuery : class;

        /// <summary>
        /// Creates caller wrapper.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <returns>Query wrapper.</returns>
        ICaller<TQuery> Query<TQuery>([NotNull] TQuery obj) where TQuery : class;
    }
}
