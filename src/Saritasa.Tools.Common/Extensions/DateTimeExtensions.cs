// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
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
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>Truncated date.</returns>
        public static DateTime Truncate(this DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)
        {
            return DateTimeUtils.Truncate(target, period, cultureInfo);
        }

        /// <summary>
        /// Compares the value of this instance to a specified object with truncation that contains a specified
        /// <see cref="System.DateTime" /> value, and returns an integer that indicates whether this instance
        /// is earlier than, the same as, or later than the specified <see cref="System.DateTime" /> value.
        /// </summary>
        /// <param name="target">The object to compare against.</param>
        /// <param name="value">The object to compare to the current instance.</param>
        /// <param name="period">Type of truncation.</param>
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>A signed number indicating the relative values of this instance and the value parameter.</returns>
        public static int CompareWithTruncate(this DateTime target, DateTime value, DateTimePeriod period, CultureInfo cultureInfo = null)
        {
            return DateTimeUtils.Truncate(target, period, cultureInfo).CompareTo(value.Truncate(period, cultureInfo));
        }
    }
}
