// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// What method to use to search class handler (for commands and events).
    /// </summary>
    public enum HandlerSearchMethod
    {
        /// <summary>
        /// The handlers class should be decorated with attrbite CommandHandlers, EventHandlers, etc.
        /// </summary>
        ClassAttribute,

        /// <summary>
        /// Class should have Handlers suffix. For example UserHandlers.
        /// </summary>
        ClassSuffix,
    }
}
