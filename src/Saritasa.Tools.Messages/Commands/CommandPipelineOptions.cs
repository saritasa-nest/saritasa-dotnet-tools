// Copyright (c) 2015-2018, Saritasa. All rights reserved.
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
        /// Default command pipeline options.
        /// </summary>
        public DefaultCommandPipelineOptions DefaultCommandPipelineOptions { get; } =
            new DefaultCommandPipelineOptions();
    }
}
