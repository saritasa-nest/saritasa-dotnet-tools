// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Default pipeline service.
    /// </summary>
    public class DefaultMessagePipelineService : IMessagePipelineService
    {
        /// <inheritdoc />
        public IMessagePipelineContainer PipelineContainer { get; set; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public DefaultMessagePipelineService()
        {
            this.ServiceProvider = NullServiceProvider.Default;
            this.PipelineContainer = new DefaultMessagePipelineContainer();
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="messagePipelineContainer">Pipelines container.</param>
        public DefaultMessagePipelineService(IServiceProvider serviceProvider,
            IMessagePipelineContainer messagePipelineContainer)
        {
            this.ServiceProvider = serviceProvider;
            this.PipelineContainer = messagePipelineContainer;
        }
    }
}
