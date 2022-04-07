// Copyright (c) 2015-2022, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Extensions;

/// <summary>
/// <see cref="IDictionary{TKey,TValue}" /> extensions.
/// </summary>
public static class DictionaryExtensions
{
#if NETSTANDARD2_0 || NETSTANDARD1_6
    /// <summary>
    /// Tries to get the value in the dictionary by key. If it does not exist it will return
    /// default value.
    /// </summary>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <param name="target">Target dictionary.</param>
    /// <param name="key">Key.</param>
    /// <param name="defaultValue">Default value.</param>
    public static TValue? GetValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue?> target,
        TKey key,
        TValue? defaultValue = default) => DictionaryUtils.GetValueOrDefault(target, key, defaultValue);
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
        this IDictionary<TKey, TValue?> target,
        TKey key,
        Func<TKey, TValue?, TValue> updateFunc,
        TValue? defaultValue = default) => DictionaryUtils.AddOrUpdate(target, key, updateFunc, defaultValue);
}
