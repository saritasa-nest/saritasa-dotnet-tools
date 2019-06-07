// Copyright (c) 2017-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.TestRuns.Internal
{
    /// <summary>
    /// Pipelines container implementation for test runs.
    /// </summary>
    public class TestRunMessagePipelineContainer : IMessagePipelineContainer
    {
        /// <inheritdoc />
        public IMessagePipeline[] Pipelines { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pipelines">Pipelines.</param>
        public TestRunMessagePipelineContainer(IEnumerable<IMessagePipeline> pipelines)
        {
            Pipelines = pipelines.ToArray();
        }
    }
}
