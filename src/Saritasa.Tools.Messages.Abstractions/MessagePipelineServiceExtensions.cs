// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Pipelines service extension methods.
    /// </summary>
    public static partial class MessagePipelineServiceExtensions
    {
        /// <summary>
        /// Get pipeline of specified type or throw exception.
        /// </summary>
        /// <typeparam name="T">Type of pipeline.</typeparam>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <returns>Pipeline.</returns>
        public static T GetPipelineOfType<T>(this IMessagePipelineService pipelineService)
            where T : class, IMessagePipeline
        {
            var container = pipelineService.PipelineContainer;
            for (int i = 0; i < container.Pipelines.Length; i++)
            {
                var pipeline = container.Pipelines[i] as T;
                if (pipeline != null)
                {
                    return pipeline;
                }
            }
            throw new InvalidOperationException($"Cannot find {typeof(T)} pipeline. Make sure you called correct " +
                                                " Add{X}Pipeline extension method.");
        }

        /// <summary>
        /// Remove pipeline of specified type or throw exception.
        /// </summary>
        /// <typeparam name="T">Pipeline type.</typeparam>
        /// <param name="pipelinesService">Pipelines service.</param>
        public static void RemovePipelineOfType<T>(this IMessagePipelineService pipelinesService)
            where T : class, IMessagePipeline
        {
            var pipeline = GetPipelineOfType<T>(pipelinesService);
            var list = pipelinesService.PipelineContainer.Pipelines.ToList();
            list.Remove(pipeline);
            pipelinesService.PipelineContainer.Pipelines = list.ToArray();
        }
    }
}
