// Copyright (c) 2015-2018, Saritasa. All rights reserved.
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
        /// <summary>
        /// Get typed object from message context items. Throws exception if not exists.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="messageContext">Message context.</param>
        /// <param name="key">Object key.</param>
        /// <exception cref="InvalidOperationException">Item with specified key not exists or has incorrect type.</exception>
        /// <returns>Item.</returns>
        public static T GetItemByKey<T>(this IMessageContext messageContext, string key)
            where T : class
        {
            if (messageContext.Items.TryGetValue(key, out object obj))
            {
                if (obj is T typedObj)
                {
                    return typedObj;
                }
            }
            throw new InvalidOperationException(
                $"The message context items dictionary expects to have item with \"{key}\" " +
                       $"and type {typeof(T).Name}.");
        }

        /// <summary>
        /// Gets typed object from message context items. Returns default value if not exists.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="messageContext">Message context.</param>
        /// <param name="key">Object key.</param>
        /// <returns>Item.</returns>
        public static T GetItemByKeyOrDefault<T>(this IMessageContext messageContext, string key)
            where T : class
        {
            if (messageContext.Items.TryGetValue(key, out object obj))
            {
                if (obj is T typedObj)
                {
                    return typedObj;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Gets object from message context items. Returns null if not exists.
        /// </summary>
        /// <param name="messageContext">Message context.</param>
        /// <param name="key">Object key.</param>
        /// <returns>Item.</returns>
        public static object GetItemByKeyOrDefault(this IMessageContext messageContext, string key)
        {
            if (messageContext.Items.TryGetValue(key, out object obj))
            {
                return obj;
            }
            return null;
        }
    }
}
