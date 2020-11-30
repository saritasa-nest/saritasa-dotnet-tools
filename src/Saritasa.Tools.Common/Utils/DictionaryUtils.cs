// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
using System.Collections.Specialized;
#endif

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Dictionary utils.
    /// </summary>
    public static class DictionaryUtils
    {
        /// <summary>
        /// Tries to get the value in the dictionary by key. If it does not exist it will return
        /// default value.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type.</typeparam>
        /// <typeparam name="TValue">Dictionary value type.</typeparam>
        /// <param name="target">Target dictionary.</param>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value by key or default value.</returns>
        public static TValue? GetValueOrDefault<TKey, TValue>(
            IDictionary<TKey, TValue?> target,
            TKey key,
            TValue defaultValue = default)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            bool success = target.TryGetValue(key, out TValue value);
            return success ? value : defaultValue;
        }

#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
        /// <summary>
        /// Tries to get the value in the <see cref="NameValueCollection" />. If value with specified
        /// key does not exist it will return default values.
        /// </summary>
        /// <param name="target">Target name value collection.</param>
        /// <param name="key">Key.</param>
        /// <param name="defaultValues">Default values.</param>
        /// <returns>Values by key or default values.</returns>
        public static string? GetValueOrDefault(
            NameValueCollection target,
            string key,
            string? defaultValues = default)
        {
            var values = target.Get(key);
            return values ?? defaultValues;
        }

        /// <summary>
        /// Tries to get the values in <see cref="NameValueCollection" />. If value with specified
        /// key does not exist it will return default values.
        /// </summary>
        /// <param name="target">Target name value collection.</param>
        /// <param name="key">Key.</param>
        /// <param name="defaultValues">Default value.</param>
        /// <returns>Values by key or default values.</returns>
        public static string[]? GetValuesOrDefault(
            NameValueCollection target,
            string key,
            string[]? defaultValues = default)
        {
            var values = target.GetValues(key);
            return values ?? defaultValues;
        }
#endif

        /// <summary>
        /// Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}" /> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="IDictionary{TKey,TValue}" /> if the key
        /// already exists. The default value will be used if the key is missed to the <see cref="IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="target">Target dictionary.</param>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="updateFunc">The function used to generate a new value for an existing key based on
        /// the key's existing value.</param>
        /// <param name="defaultValue">The value to be used for an absent key.</param>
        /// <returns>The new or updated value for the key.</returns>
        public static TValue? AddOrUpdate<TKey, TValue>(
            IDictionary<TKey, TValue?> target,
            TKey key,
            Func<TKey, TValue?, TValue> updateFunc,
            TValue defaultValue = default)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (updateFunc == null)
            {
                throw new ArgumentNullException(nameof(updateFunc));
            }

            var keyExists = target.TryGetValue(key, out TValue value);
            if (keyExists)
            {
                value = updateFunc(key, value);
                target[key] = value;
            }
            else
            {
                value = updateFunc(key, defaultValue);
                target.Add(key, value);
            }
            return value;
        }
    }
}
