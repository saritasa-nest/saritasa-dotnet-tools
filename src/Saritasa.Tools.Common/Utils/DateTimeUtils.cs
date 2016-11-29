// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Date time utils.
    /// </summary>
    public static class DateTimeUtils
    {
        /// <summary>
        /// Returns dates range. Uses lazy implementation.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns>Dates range.</returns>
        public static IEnumerable<DateTime> Range(DateTime fromDate, DateTime toDate)
        {
            return Enumerable.Range(0, toDate.Subtract(fromDate).Days + 1).Select(d => fromDate.AddDays(d));
        }
    }
}
