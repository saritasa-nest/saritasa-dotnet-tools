// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Basic pipeline options.
    /// </summary>
    public class MessagePipelineOptions
    {
        /// <summary>
        /// Throw exception if execution were failes.
        /// </summary>
        public bool ThrowExceptionOnFail { get; set; } = true;

        /// <summary>
        /// Include execution duration to message context items.
        /// </summary>
        public bool IncludeExecutionDuration { get; set; } = true;
    }
}
