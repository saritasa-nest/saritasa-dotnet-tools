// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Pipeline builder class that contain basic functionality.
    /// </summary>
    /// <typeparam name="TPipeline">Target pipeline type.</typeparam>
    public class BasePipelineBuilder<TPipeline> where TPipeline : class, IMessagePipeline
    {
        /// <summary>
        /// Pipeline to build.
        /// </summary>
        protected TPipeline Pipeline { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="pipeline">Message pipeline.</param>
        public BasePipelineBuilder(TPipeline pipeline)
        {
            this.Pipeline = pipeline;
        }

        internal void AddMiddlewareInternal(IMessagePipelineMiddleware middleware)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }
            if (Pipeline.Middlewares == null)
            {
                Pipeline.Middlewares = new IMessagePipelineMiddleware[1];
                return;
            }
            var list = Pipeline.Middlewares.ToList();
            list.Add(middleware);
            Pipeline.Middlewares = list.ToArray();
        }
    }
}
