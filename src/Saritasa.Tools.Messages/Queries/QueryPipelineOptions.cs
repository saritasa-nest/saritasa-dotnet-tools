// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// Query pipeline options.
    /// </summary>
    public class QueryPipelineOptions
    {
        /// <summary>
        /// Setup default query pipeline middlewares.
        /// </summary>
        public DefaultQueryPipelineOptions DefaultQueryPipelineOptions { get; set; } =
            new DefaultQueryPipelineOptions();
    }
}
