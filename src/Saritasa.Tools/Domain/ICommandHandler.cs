// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand> :
        ICommandHandler where TCommand : ICommand
    {
        /// <summary>
        /// Body of command.
        /// </summary>
        /// <param name="command">Command.</param>
        void Handle(TCommand command);
    }

    /// <summary>
    /// The class that contains commands handers should be marked
    /// with this interface.
    /// </summary>
    public interface ICommandHandler
    {
    }
}
