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
        /// <returns>Start of period date.</returns>
        public static DateTime StartOf(DateTime target, DateTimePeriod period)
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
                case DateTimePeriod.Second:
                    result = result.AddSeconds(1);
                    break;
                case DateTimePeriod.Minute:
                    result = result.AddMinutes(1);
                    break;
                case DateTimePeriod.Hour:
                    result = result.AddHours(1);
                    break;
                case DateTimePeriod.Day:
                    result = result.AddDays(1);
                    break;
                case DateTimePeriod.Week:
                    result = result.AddDays(7);
                    break;
                case DateTimePeriod.Month:
                    result = result.AddMonths(1);
                    break;
                case DateTimePeriod.Quarter:
                    result = result.AddMonths(4);
                    break;
                case DateTimePeriod.Year:
                    result = result.AddYears(1);
                    break;
                case DateTimePeriod.None:
                default:
                    return target;
            }
            return result.AddMilliseconds(-1);
        }

        /// <summary>
        /// Shortcut to set date part.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period to replace.</param>
        /// <param name="value">Value to replace by.</param>
        /// <returns>The date with new value.</returns>
        public static DateTime Set(DateTime target, DateTimePeriod period, int value)
        {
            switch (period)
            {
                case DateTimePeriod.Second:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, value,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Minute:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, value, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Hour:
                    return new DateTime(target.Year, target.Month, target.Day, value, target.Minute, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Day:
                    return new DateTime(target.Year, target.Month, value, target.Hour, target.Minute, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Week:
                    throw new ArgumentException(
                        (Properties.Strings.ArgumentCannotBeThePeriod, period), nameof(period));
                case DateTimePeriod.Month:
                    return new DateTime(target.Year, value, target.Day, target.Hour, target.Minute, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Quarter:
                    throw new ArgumentException(string.Format(Properties.Strings.ArgumentCannotBeThePeriod, period.ToString()), nameof(period));
                case DateTimePeriod.Year:
                    return new DateTime(value, target.Month, target.Day, target.Hour, target.Minute, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.None:
                default:
                    return target;
            }
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

        /// <summary>
        /// Return period difference between two dates. Negative values are converted to positive.
        /// </summary>
        /// <param name="target1">Date 1.</param>
        /// <param name="target2">Date 2.</param>
        /// <param name="period">Period type.</param>
        /// <returns>Difference.</returns>
        public static double Diff(DateTime target1, DateTime target2, DateTimePeriod period)
        {
            // Swap to get positive value.
            if (target1 > target2)
            {
                return Diff(target2, target1, period);
            }

            switch (period)
            {
                case DateTimePeriod.Second:
                    return (target2 - target1).TotalSeconds;
                case DateTimePeriod.Minute:
                    return (target2 - target1).TotalMinutes;
                case DateTimePeriod.Hour:
                    return (target2 - target1).TotalHours;
                case DateTimePeriod.Day:
                    return (target2 - target1).TotalDays;
                case DateTimePeriod.Week:
                    return (target2 - target1).TotalDays / 7;
                case DateTimePeriod.Month:
                    return MonthDiff(target2, target1);
                case DateTimePeriod.Quarter:
                    return MonthDiff(target2, target1) / 3;
                case DateTimePeriod.Year:
                    return MonthDiff(target2, target1) / 12;
            }

            throw new ArgumentException(nameof(period));
        }

        /// <summary>
        /// Calculate difference between two dates in months as float value.
        /// </summary>
        /// <remarks>
        /// Original: https://github.com/moment/moment/blob/develop/src/lib/moment/diff.js .
        /// </remarks>
        /// <param name="target1">Date 1.</param>
        /// <param name="target2">Date 2.</param>
        /// <returns>Months difference.</returns>
        private static double MonthDiff(DateTime target1, DateTime target2)
        {
            // Difference in months.
            var wholeMonthDiff = (target2.Year - target1.Year) * 12 + (target2.Month - target1.Month);

            // b is in (anchor - 1 month, anchor + 1 month).
            DateTime anchor = target1.AddMonths(wholeMonthDiff);
            DateTime anchor2;
            double adjust;

            if (target2 - anchor < TimeSpan.Zero)
            {
                anchor2 = target1.AddMonths(wholeMonthDiff - 1);
                // Linear across the month.
                adjust = (target2 - anchor).TotalMilliseconds / (anchor - anchor2).TotalMilliseconds;
            }
            else
            {
                anchor2 = target1.AddMonths(wholeMonthDiff + 1);
                // Linear across the month.
                adjust = (target2 - anchor).TotalMilliseconds / (anchor2 - anchor).TotalMilliseconds;
            }

            return -(wholeMonthDiff + adjust);
        }
    }
}
