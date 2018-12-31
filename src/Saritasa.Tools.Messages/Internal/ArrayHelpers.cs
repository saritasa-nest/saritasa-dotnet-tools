// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// Array helpers.
    /// </summary>
    internal static class ArrayHelpers
    {
        internal static T[] AddItem<T>(T[] arr, T item)
        {
            if (arr == null)
            {
                return new[] { item };
            }
            var sourceLength = arr.Length;
            var newarr = new T[sourceLength + 1];
            Array.Copy(arr, newarr, sourceLength);
            newarr[sourceLength] = item;
            return newarr;
        }

        internal static T[] AddItems<T>(T[] arr, T[] items)
        {
            if (arr == null)
            {
                return items;
            }
            var sourceLength = arr.Length;
            var newLength = sourceLength + items.Length;
            var newarr = new T[newLength];
            Array.Copy(arr, newarr, sourceLength);
            for (int i = 0; i < items.Length; i++)
            {
                newarr[sourceLength + i] = items[i];
            }
            return newarr;
        }
    }
}
