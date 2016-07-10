// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;

    /// <summary>
    /// Saves the command execution context to repository.
    /// </summary>
    public class CommandRepositoryMiddleware : ICommandPipelineMiddleware
    {
        /// <inheritdoc />
        public string Id
        {
            get { return "repository"; }
        }

        ICommandRepository repository;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="repository">Repository implementation.</param>
        public CommandRepositoryMiddleware(ICommandRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
            this.repository = repository;
        }

        /// <inheritdoc />
        public void Execute(CommandExecutionContext context)
        {
            repository.Add(context.GetResult());
        }
    }
}
