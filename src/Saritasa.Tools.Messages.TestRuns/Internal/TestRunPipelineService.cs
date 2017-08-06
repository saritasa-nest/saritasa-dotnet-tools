// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.TestRuns.Internal
{
    /// <summary>
    /// Simple implementation of pipeline service for test run.
    /// </summary>
    public class TestRunPipelineService : IMessagePipelineService
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public TestRunPipelineService()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="pipelines">Pipelines.</param>
        public TestRunPipelineService(IServiceProvider serviceProvider, params IMessagePipeline[] pipelines)
        {
            this.ServiceProvider = serviceProvider;
            this.PipelineContainer = new TestRunMessagePipelineContainer(pipelines);
        }

        /// <inheritdoc />
        public IMessagePipelineContainer PipelineContainer { get; set; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; set; }
    }
}
