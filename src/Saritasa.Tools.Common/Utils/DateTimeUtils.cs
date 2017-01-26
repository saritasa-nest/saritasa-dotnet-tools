// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

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

        /// <summary>
        /// Combines date part from first date and time from another.
        /// </summary>
        /// <param name="date">Date part.</param>
        /// <param name="time">Time part.</param>
        /// <returns>Combined DateTime.</returns>
        public static DateTime CombineDateTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Kind);
        }

        /// <summary>
        /// Begin datetime of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period type.</param>
        /// <returns>Begin of period date.</returns>
        public static DateTime BeginOf(DateTime target, DateTimePeriod period)
        {
            return target.Truncate(period);
        }

        /// <summary>
        /// End datetime of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period type.</param>
        /// <returns>End of period date.</returns>
        public static DateTime EndOf(DateTime target, DateTimePeriod period)
        {
            var result = target.Truncate(period);
            switch (period)
            {
                case DateTimePeriod.Seconds:
                    result = result.AddSeconds(1);
                    break;
                case DateTimePeriod.Minutes:
                    result = result.AddMinutes(1);
                    break;
                case DateTimePeriod.Hours:
                    result = result.AddHours(1);
                    break;
                case DateTimePeriod.Days:
                    result = result.AddDays(1);
                    break;
                case DateTimePeriod.Weeks:
                    result = result.AddDays(7);
                    break;
                case DateTimePeriod.Months:
                    result = result.AddMonths(1);
                    break;
                case DateTimePeriod.Quarters:
                    result = result.AddMonths(4);
                    break;
                case DateTimePeriod.Years:
                    result = result.AddYears(1);
                    break;
                case DateTimePeriod.None:
                default:
                    return target;
            }
            return result.AddMilliseconds(-1);
        }

        #region Unix

        /// <summary>
        /// Unix epoch.
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts from unix time stamp to <see cref="DateTime" />.
        /// </summary>
        /// <param name="unixTimeStamp">Unix time stamp.</param>
        /// <returns>Datetime.</returns>
        public static DateTime FromUnixTimestamp(double unixTimeStamp)
        {
            return UnixEpoch.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        /// <summary>
        /// Converts <see cref="DateTime" /> to unix time stamp.
        /// </summary>
        /// <param name="target">Target datetime</param>
        /// <returns>Unix time stamp.</returns>
        public static double ToUnixTimestamp(DateTime target)
        {
            return (target - UnixEpoch).TotalMilliseconds;
        }

        #endregion
    }
}
