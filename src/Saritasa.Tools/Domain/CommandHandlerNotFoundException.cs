// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;

    public class CommandHandlerNotFoundException : Exception
    {
        const string msg = "Cannot find command handler or it cannot be resolved. Make sure it has default public parameterless " +
            "constructor or registered with your dependency injection container.";

        public CommandHandlerNotFoundException() :
            base(msg)
        {
        }
    }
}
