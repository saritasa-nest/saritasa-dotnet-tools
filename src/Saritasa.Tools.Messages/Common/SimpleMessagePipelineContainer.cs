// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Simple message pipeline container with default arrays implementation.
    /// </summary>
    public class SimpleMessagePipelineContainer : IMessagePipelineContainer
    {
        /// <summary>
        /// Default static instance.
        /// </summary>
        public static readonly IMessagePipelineContainer Default = new SimpleMessagePipelineContainer();

        /// <inheritdoc />
        public IMessagePipeline[] Pipelines { get; set; } = new IMessagePipeline[0];

        /// <summary>
        /// .ctor
        /// </summary>
        public SimpleMessagePipelineContainer()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="pipelines">Pipelines enumerable.</param>
        public SimpleMessagePipelineContainer(IEnumerable<IMessagePipeline> pipelines)
        {
            if (pipelines == null)
            {
                throw new ArgumentNullException(nameof(pipelines));
            }
            this.Pipelines = pipelines.ToArray();
        }
    }
}
