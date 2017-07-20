// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Commands
{
    /// <summary>
    /// Command pipeline options.
    /// </summary>
    public class CommandPipelineOptions
    {
        /// <summary>
        /// Create command handlers without trying to resolve using context
        /// service provider.
        /// </summary>
        public bool CreateCommandHandlers { get; set; } = true;
    }
}
