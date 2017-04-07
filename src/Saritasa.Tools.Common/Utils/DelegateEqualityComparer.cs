// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Delegate equality comparer.
    /// </summary>
    /// <typeparam name="TSource">The type of source elements.</typeparam>
    /// <typeparam name="TKey">The type of key to equal.</typeparam>
    internal class DelegateEqualityComparer<TSource, TKey> : IEqualityComparer<TSource>
    {
        private readonly Func<TSource, TKey> selector;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="selector">Selector delegate.</param>
        public DelegateEqualityComparer(Func<TSource, TKey> selector)
        {
            this.selector = selector;
        }

        /// <inheritdoc />
        public bool Equals(TSource x, TSource y)
        {
            return Equals(selector(x), selector(y));
        }

        /// <inheritdoc />
        public int GetHashCode(TSource obj)
        {
            return selector(obj).GetHashCode();
        }
    }
}
