// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Commands
{
    using System;

    /// <summary>
    /// Occures when command handler cannot be found.
    /// </summary>
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class CommandHandlerNotFoundException : Exception
    {
        const string Msg = "Cannot find command handler or it cannot be resolved. Make sure it has default public parameterless " +
            "constructor or registered with your dependency injection container.";

        /// <summary>
        /// .ctor
        /// </summary>
        public CommandHandlerNotFoundException() : base(Msg)
        {
        }
    }
}
