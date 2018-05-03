// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.ExceptionServices;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Command pipeline options.
    /// </summary>
    public class CommandPipelineOptions : BaseAssembliesOptions
    {
        /// <summary>
        /// Create default middlewares. <c>True</c> by default.
        /// </summary>
        public bool UseDefaultPipeline { get; set; } = true;

        /// <summary>
        /// Throw exception if execution was failed.
        /// By default <see cref="Saritasa.Tools.Messages.Abstractions.MessageProcessingException" />
        /// will be thrown.
        /// </summary>
        public bool ThrowExceptionOnFail { get; set; } = true;

        /// <summary>
        /// Captures <see cref="ExceptionDispatchInfo" /> of original execution exception
        /// as item with ".exception-dispatch" key. Default is <c>false</c>.
        /// </summary>
        public bool UseExceptionDispatchInfo { get; set; } = false;

        /// <summary>
        /// Options for internal service resolver.
        /// </summary>
        public InternalResolverOptions InternalResolver { get; set; } = new InternalResolverOptions();
    }
}
