// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace Saritasa.Tools.Common.Utils
{
#if NETSTANDARD1_6 || NETSTANDARD2_0 || NETSTANDARD2_1
    /// <summary>
    /// Order entries parsing strategies to get array of (fieldName, order) tuples.
    /// </summary>
    public static class OrderParsingDelegates
    {
        /// <summary>
        /// Parse string like "name:asc,age:desc". Can be used for transfer thru query strings.
        /// </summary>
        /// <param name="orderQuery">Order query string.</param>
        /// <returns>Ordering entries.</returns>
        public static (string FieldName, ListSortDirection Order)[] ParseSeparated(string orderQuery)
        {
            if (orderQuery == null)
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
                throw new InvalidOperationException(string.Format(Properties.Strings.InvalidOrderDirection, order));
            }

            if (string.IsNullOrWhiteSpace(orderQuery))
            {
                return Array.Empty<(string FieldName, ListSortDirection Order)>();
            }

            var sortingRecordsStrings = orderQuery.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var arr = new (string FieldName, ListSortDirection Order)[sortingRecordsStrings.Length];
            for (int i = 0; i < sortingRecordsStrings.Length; i++)
            {
                var ind = sortingRecordsStrings[i].IndexOf(':');
                if (ind > -1)
                {
                    arr[i] = (sortingRecordsStrings[i].Substring(0, ind), ParseOrder(sortingRecordsStrings[i].Substring(ind + 1)));
                }
                else
                {
                    arr[i] = (sortingRecordsStrings[i], ListSortDirection.Ascending);
                }
            }
            return arr;
        }
    }
#endif
}
