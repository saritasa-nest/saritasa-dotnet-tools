// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Message pipeline post action.
    /// </summary>
    public interface IMessagePipelinePostAction
    {
        /// <summary>
        /// Post handler.
        /// </summary>
        /// <param name="messageContext">Message context.</param>
        void PostHandle(IMessageContext messageContext);
    }
}
