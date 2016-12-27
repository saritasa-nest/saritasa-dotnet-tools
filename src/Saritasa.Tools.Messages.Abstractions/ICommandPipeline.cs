// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Abstractions
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Commands specific pipeline.
    /// </summary>
    public interface ICommandPipeline : IMessagePipeline
    {
        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        void Handle(object command);

        /// <summary>
        /// Execute command asynchronously.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        Task HandleAsync(object command);
    }
}
