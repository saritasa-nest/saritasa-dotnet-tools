// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.ExceptionServices;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// Query pipeline options.
    /// </summary>
    public class QueryPipelineOptions
    {
        /// <summary>
        /// Throw exception if execution was failed.
        /// By default <see cref="Saritasa.Tools.Messages.Abstractions.MessageProcessingException" />
        /// will be thrown. <c>True</c> by default.
        /// </summary>
        public bool ThrowExceptionOnFail { get; set; } = true;

        /// <summary>
        /// Captures <see cref="ExceptionDispatchInfo" /> of original execution exception
        /// as item with ".exception-dispatch" key. Default is <c>false</c>.
        /// </summary>
        public bool UseExceptionDispatchInfo { get; set; } = false;

        /// <summary>
        /// Calculate execution duration time. <c>True</c> by default.
        /// </summary>
        public bool IncludeExecutionDuration { get; set; } = true;

        /// <summary>
        /// Options for internal service resolver.
        /// </summary>
        public QueryPipelineInternalResolverOptions InternalResolver { get; set; } = new QueryPipelineInternalResolverOptions();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public QueryPipelineOptions()
        {
            InternalResolver.UseHandlerParametersResolve = false;
        }
    }
}
