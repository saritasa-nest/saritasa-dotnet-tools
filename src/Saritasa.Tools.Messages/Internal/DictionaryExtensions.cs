// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// Dictionary extensions.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Tries to get the value in dictionary by key. If it does not exist it will return
        /// default value.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type.</typeparam>
        /// <typeparam name="TValue">Dictionary value type.</typeparam>
        /// <param name="target">Target dictionary.</param>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">Default value.</param>
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> target,
            TKey key,
            TValue defaultValue = default(TValue))
        {
            TValue value;
            bool success = target.TryGetValue(key, out value);
            return success ? value : defaultValue;
        }

        /// <summary>
        /// Tries to get the value in dictionary by key. If it does not exist it will invoke action.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type.</typeparam>
        /// <typeparam name="TValue">Dictionary value type.</typeparam>
        /// <param name="target">Target dictionary.</param>
        /// <param name="key">Key.</param>
        /// <param name="action">Action to execure if key not exists..</param>
        public static TValue GetValueOrInvoke<TKey, TValue>(
            this IDictionary<TKey, TValue> target,
            TKey key,
            Action<TKey> action)
        {
            TValue value;
            bool success = target.TryGetValue(key, out value);
            if (success)
            {
                return value;
            }
            action(key);
            return default(TValue);
        }
    }
}
