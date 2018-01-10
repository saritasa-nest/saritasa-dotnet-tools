// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Events
{
    /// <summary>
    /// Event pipeline options.
    /// </summary>
    public class EventPipelineOptions
    {
        /// <summary>
        /// Setup default event pipeline options.
        /// </summary>
        public DefaultEventPipelineOptions DefaultEventPipelineOptions { get; set; } =
            new DefaultEventPipelineOptions();
    }
}
