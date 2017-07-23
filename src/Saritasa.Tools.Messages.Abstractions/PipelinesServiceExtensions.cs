// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Pipelines service extension methods.
    /// </summary>
    public static class PipelinesServiceExtensions
    {
        /// <summary>
        /// Get pipeline of specified type or throw exception.s
        /// </summary>
        /// <typeparam name="T">Type of pipeline.</typeparam>
        /// <param name="pipelinesService">Pipelines service.</param>
        /// <returns>Pipeline.</returns>
        public static T GetPipelineOfType<T>(this IPipelinesService pipelinesService)
            where T : class, IMessagePipeline
        {
            for (int i = 0; i < pipelinesService.Pipelines.Length; i++)
            {
                var pipeline = pipelinesService.Pipelines[i] as T;
                if (pipeline != null)
                {
                    return pipeline;
                }
            }
            throw new InvalidOperationException("Cannot find command pipeline. Make sure you called any of AddPipeline " +
                                                "extension method.");
        }

        /// <summary>
        /// Remove pipeline of specified type or throw exception.
        /// </summary>
        /// <typeparam name="T">Pipeline type.</typeparam>
        /// <param name="pipelinesService">Pipelines service.</param>
        public static void RemovePipelineOfType<T>(this IPipelinesService pipelinesService)
            where T : class, IMessagePipeline
        {
            var pipeline = GetPipelineOfType<T>(pipelinesService);
            var list = pipelinesService.Pipelines.ToList();
            list.Remove(pipeline);
            pipelinesService.Pipelines = list.ToArray();
        }
    }
}
