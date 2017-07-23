// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Default pipelines service.
    /// </summary>
    public class DefaultPipelinesService : IPipelinesService
    {
        /// <inheritdoc />
        public IMessagePipeline[] Pipelines { get; set; } = new IMessagePipeline[0];

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; set; } = new NullServiceProvider();

        /// <inheritdoc />
        public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
    }
}
