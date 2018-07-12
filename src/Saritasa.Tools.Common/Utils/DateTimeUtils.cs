// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Saritasa.Tools.Common.Extensions;

namespace Saritasa.Tools.Common.Utils
{
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
        public static IEnumerable<DateTime> GetRange(DateTime fromDate, DateTime toDate)
        {
            return Enumerable.Range(0, toDate.Subtract(fromDate).Days + 1).Select(d => fromDate.AddDays(d));
        }

        /// <summary>
        /// Combines date part from first date and time from another. Kind is taken from time part.
        /// Kind is getting from time part.
        /// </summary>
        /// <param name="date">Date part.</param>
        /// <param name="time">Time part.</param>
        /// <returns>Combined DateTime.</returns>
        /// <exception cref="ArgumentException">Date and time kind format is different.</exception>
        public static DateTime CombineDateAndTime(DateTime date, DateTime time)
        {
            if (date.Kind != time.Kind)
            {
                throw new ArgumentException(Properties.Strings.ArgumentDateTimeKindMustBeEqual);
            }
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Kind);
        }

        /// <summary>
        /// Returns begin <see cref="System.DateTime" /> of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period type.</param>
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>Start of period date.</returns>
        public static DateTime GetStartOfPeriod(DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)
        {
            return target.Truncate(period, cultureInfo);
        }

        /// <summary>
        /// Returns end <see cref="System.DateTime" /> of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period type.</param>
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>End of period date.</returns>
        public static DateTime GetEndOfPeriod(DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)
        {
            var result = target.Truncate(period);
            switch (period)
            {
                case DateTimePeriod.Millisecond:
                    result = result.AddMilliseconds(1);
                    break;
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
                case DateTimePeriod.Decade:
                    result = result.AddYears(10);
                    break;
                case DateTimePeriod.None:
                default:
                    return target;
            }
            return result.AddMilliseconds(-1);
        }

        /// <summary>
        /// Shortcut to set date part. Method throws <see cref="System.ArgumentException" /> for
        /// week and quarter periods.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period to replace.</param>
        /// <param name="value">Value to replace by.</param>
        /// <returns>The date with new value.</returns>
        public static DateTime SetPart(DateTime target, DateTimePeriod period, int value)
        {
            switch (period)
            {
                case DateTimePeriod.Millisecond:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, target.Second,
                        value, target.Kind);
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
                        String.Format(Properties.Strings.ArgumentCannotBeThePeriod, period), nameof(period));
                case DateTimePeriod.Month:
                    return new DateTime(target.Year, value, target.Day, target.Hour, target.Minute, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Quarter:
                    throw new ArgumentException(string.Format(Properties.Strings.ArgumentCannotBeThePeriod, period), nameof(period));
                case DateTimePeriod.Year:
                    return new DateTime(value, target.Month, target.Day, target.Hour, target.Minute, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Decade:
                    throw new ArgumentException(
                        String.Format(Properties.Strings.ArgumentCannotBeThePeriod, period), nameof(period));
                case DateTimePeriod.None:
                default:
                    return target;
            }
        }

        /// <summary>
        /// Truncates date by specified period. Returns begin of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Type of truncation.</param>
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>Truncated date.</returns>
        public static DateTime Truncate(DateTime target, DateTimePeriod period, CultureInfo cultureInfo = null)
        {
            // For reference https://www.postgresql.org/docs/10/static/functions-datetime.html#FUNCTIONS-DATETIME-TRUNC
            switch (period)
            {
                case DateTimePeriod.Millisecond:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, target.Second,
                        target.Millisecond, target.Kind);
                case DateTimePeriod.Second:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, target.Second, target.Kind);
                case DateTimePeriod.Minute:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, 0, target.Kind);
                case DateTimePeriod.Hour:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, 0, 0, target.Kind);
                case DateTimePeriod.Day:
                    return new DateTime(target.Year, target.Month, target.Day, 0, 0, 0, target.Kind);
                case DateTimePeriod.Week:
                    var firstDayOfWeek = cultureInfo?.DateTimeFormat.FirstDayOfWeek ?? CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                    int diff = (7 + (target.DayOfWeek - firstDayOfWeek)) % 7;
                    return new DateTime(target.Year, target.Month, target.Day, 0, 0, 0, target.Kind).AddDays(-1 * diff);
                case DateTimePeriod.Month:
                    return new DateTime(target.Year, target.Month, 1, 0, 0, 0, target.Kind);
                case DateTimePeriod.Quarter:
                    return new DateTime(target.Year, target.Month, 1, 0, 0, 0, target.Kind).AddMonths(-(target.Month - 1) % 3);
                case DateTimePeriod.Year:
                    return new DateTime(target.Year, 1, 1, 0, 0, 0, target.Kind);
                case DateTimePeriod.Decade:
                    return new DateTime(target.Year - (target.Year % 10), 1, 1, 0, 0, 0, target.Kind);
                case DateTimePeriod.None:
                default:
                    return target;
            }
        }

        /// <summary>
        /// Compares the value of the instance to a specified <see cref="System.DateTime" /> object with truncation,
        /// and returns an integer that indicates whether this instance
        /// is earlier than, the same as, or later than the specified <see cref="System.DateTime" /> value.
        /// </summary>
        /// <param name="target">The date to compare against.</param>
        /// <param name="value">The date to compare to the current instance.</param>
        /// <param name="period">Type of truncation.</param>
        /// <returns>A signed number indicating the relative values of this instance and the value parameter.</returns>
        public static int CompareTo(DateTime target, DateTime value, DateTimePeriod period)
        {
            return Truncate(target, period).CompareTo(value.Truncate(period));
        }

        #region Unix

        /// <summary>
        /// Unix epoch.
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts from unix time stamp to <see cref="System.DateTime" />.
        /// </summary>
        /// <param name="unixTimeStamp">Unix time stamp.</param>
        /// <returns>Datetime.</returns>
        public static DateTime FromUnixTimestamp(double unixTimeStamp)
        {
            return UnixEpoch.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        /// <summary>
        /// Converts <see cref="System.DateTime" /> to unix time stamp.
        /// </summary>
        /// <param name="target">Target datetime.</param>
        /// <returns>Unix time stamp.</returns>
        public static double ToUnixTimestamp(DateTime target)
        {
            return (target - UnixEpoch).TotalSeconds;
        }

        #endregion

        /// <summary>
        /// Return period difference between two dates. Negative values are converted to positive.
        /// </summary>
        /// <param name="target1">Date 1.</param>
        /// <param name="target2">Date 2.</param>
        /// <param name="period">Period type.</param>
        /// <returns>Difference.</returns>
        public static double GetDiff(DateTime target1, DateTime target2, DateTimePeriod period)
        {
            // Swap to get positive value.
            if (target1 > target2)
            {
                return GetDiff(target2, target1, period);
            }

            switch (period)
            {
                case DateTimePeriod.Millisecond:
                    return (target2 - target1).TotalMilliseconds;
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
                    return GetMonthDiff(target2, target1);
                case DateTimePeriod.Quarter:
                    return GetMonthDiff(target2, target1) / 3;
                case DateTimePeriod.Year:
                    return GetMonthDiff(target2, target1) / 12;
                case DateTimePeriod.Decade:
                    return GetMonthDiff(target2, target1) / 12 * 10;
            }

            throw new ArgumentException(nameof(period));
        }

        /// <summary>
        /// Calculate difference between two dates in months as double value.
        /// </summary>
        /// <remarks>
        /// Original: https://github.com/moment/moment/blob/develop/src/lib/moment/diff.js .
        /// </remarks>
        /// <param name="target1">Date 1.</param>
        /// <param name="target2">Date 2.</param>
        /// <returns>Months difference.</returns>
        private static double GetMonthDiff(DateTime target1, DateTime target2)
        {
            // Difference in months.
            var wholeMonthDiff = (target2.Year - target1.Year) * 12 + (target2.Month - target1.Month);

            // target2 is in (anchor - 1 month, anchor + 1 month).
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
