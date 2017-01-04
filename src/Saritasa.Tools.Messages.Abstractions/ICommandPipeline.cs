// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Abstractions
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>
    /// Commands specific pipeline.
    /// </summary>
    public interface ICommandPipeline : IMessagePipeline
    {
        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        void Handle([NotNull] object command);

        /// <summary>
        /// Execute command asynchronously.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        Task HandleAsync([NotNull] object command);
    }
}
