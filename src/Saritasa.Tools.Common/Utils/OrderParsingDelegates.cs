// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Order parsing strategies to get array of <see cref="OrderingEntry" />.
    /// </summary>
    public static class OrderParsingDelegates
    {
        /// <summary>
        /// Parse string like "name:asc,age:desc". Can be used for transfer thru query strings.
        /// </summary>
        /// <param name="orderQuery">Order query string.</param>
        /// <returns>Ordering entries.</returns>
        public static OrderingEntry[] ParseSeparated(string orderQuery)
        {
            if (string.IsNullOrEmpty(orderQuery))
            {
                throw new ArgumentNullException(nameof(orderQuery));
            }

            ListSortDirection ParseOrder(string order)
            {
                if (order.StartsWith("asc", StringComparison.OrdinalIgnoreCase))
                {
                    return ListSortDirection.Ascending;
                }
                if (order.StartsWith("desc", StringComparison.OrdinalIgnoreCase))
                {
                    return ListSortDirection.Descending;
                }
                throw new InvalidOperationException($"Cannot recognize sort direction \"{order}\".");
            }

            var sortingRecordsStrings = orderQuery.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var arr = new OrderingEntry[sortingRecordsStrings.Length];
            for (int i = 0; i < sortingRecordsStrings.Length; i++)
            {
                var ind = sortingRecordsStrings[i].IndexOf(':');
                if (ind > -1)
                {
                    arr[i] = new OrderingEntry(sortingRecordsStrings[i].Substring(0, ind),
                        ParseOrder(sortingRecordsStrings[i].Substring(ind + 1)));
                }
                else
                {
                    arr[i] = new OrderingEntry(sortingRecordsStrings[i]);
                }
            }
            return arr;
        }
    }
}
