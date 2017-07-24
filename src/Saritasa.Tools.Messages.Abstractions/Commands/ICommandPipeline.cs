// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Abstractions.Commands
{
    /// <summary>
    /// Commands specific pipeline.
    /// </summary>
    public interface ICommandPipeline : IMessagePipeline
    {
        /// <summary>
        /// Create message context from pipelines service and command object.
        /// </summary>
        /// <param name="pipelineService">Pipelines service.</param>
        /// <param name="command">Command object.</param>
        /// <returns>Message context.</returns>
        IMessageContext CreateMessageContext([NotNull] IPipelineService pipelineService, [NotNull] object command);
    }
}
