// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Message context extensions.
    /// </summary>
    public static class MessageContextExtensions
    {
        private static readonly object lockObj = new object();

        /// <summary>
        /// Add item to global items collections in thread safe mode.
        /// </summary>
        /// <param name="messageContext">Message context to use.</param>
        /// <param name="key">Key.</param>
        /// <param name="valueFunc">Value factory.</param>
        public static void AddGlobalItemSafe(this IMessageContext messageContext, object key, Func<object> valueFunc)
        {
            lock (lockObj)
            {
                messageContext.GlobalItems[key] = valueFunc();
            }
        }
    }
}
