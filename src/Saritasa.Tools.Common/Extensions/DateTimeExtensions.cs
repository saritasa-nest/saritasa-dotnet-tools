// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Extensions
{
    /// <summary>
    /// Helper methods for DateTime class.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Truncates date. Begin of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Type of truncation.</param>
        /// <returns>Truncated date.</returns>
        public static DateTime Truncate(this DateTime target, DateTimePeriod period)
        {
            return DateTimeUtils.Truncate(target, period);
        }

        /// <summary>
        /// Is date between two startDate and endDate dates shortcut method.
        /// </summary>
        /// <param name="target">Date to compare.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>True if date between startDate and endDate, False otherwise.</returns>
        public static bool IsBetween(this DateTime target, DateTime startDate, DateTime endDate)
        {
            return DateTimeUtils.IsBetween(target, startDate, endDate);
        }

        /// <summary>
        /// Compares the value of this instance to a specified object with truncation that contains a specified
        /// <see cref="System.DateTime" /> value, and returns an integer that indicates whether this instance
        /// is earlier than, the same as, or later than the specified <see cref="System.DateTime" /> value.
        /// </summary>
        /// <param name="target">The object to compare against.</param>
        /// <param name="value">The object to compare to the current instance.</param>
        /// <param name="period">Type of truncation.</param>
        /// <returns>A signed number indicating the relative values of this instance and the value parameter.</returns>
        public static int CompareTo(this DateTime target, DateTime value, DateTimePeriod period)
        {
            return DateTimeUtils.Truncate(target, period).CompareTo(value.Truncate(period));
        }
    }
}
