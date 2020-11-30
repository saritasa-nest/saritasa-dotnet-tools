// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Date time utils. Overloads for <see cref="DateTimeOffset" />.
    /// </summary>
    public static partial class DateTimeUtils
    {
        /// <summary>
        /// Returns dates range. Uses lazy implementation.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="period">Period, the interval between dates.</param>
        /// <returns>Dates range.</returns>
        public static IEnumerable<DateTimeOffset> GetRange(DateTimeOffset fromDate, DateTimeOffset toDate, DateTimePeriod period = DateTimePeriod.Day)
        {
            if (fromDate <= toDate)
            {
                for (var currentDate = fromDate; currentDate <= toDate; currentDate = Add(currentDate, period, 1))
                {
                    yield return currentDate;
                }
            }
            else
            {
                for (var currentDate = fromDate; currentDate >= toDate; currentDate = Add(currentDate, period, -1))
                {
                    yield return currentDate;
                }
            }
        }

        /// <summary>
        /// Add to target date a specific amount of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period to add.</param>
        /// <param name="value">The specific amount to add.</param>
        /// <returns>New modified date value.</returns>
        public static DateTimeOffset Add(DateTimeOffset target, DateTimePeriod period, double value)
        {
            switch (period)
            {
                case DateTimePeriod.Millisecond:
                    return target.AddMilliseconds(value);
                case DateTimePeriod.Second:
                    return target.AddSeconds(value);
                case DateTimePeriod.Minute:
                    return target.AddMinutes(value);
                case DateTimePeriod.Hour:
                    return target.AddHours(value);
                case DateTimePeriod.Day:
                    return target.AddDays(value);
                case DateTimePeriod.Week:
                    return target.AddDays(value * 7);
                case DateTimePeriod.Month:
                    return target.AddMonths((int)value);
                case DateTimePeriod.Quarter:
                    return target.AddMonths((int)value * 4);
                case DateTimePeriod.Year:
                    return target.AddYears((int)value);
                case DateTimePeriod.Decade:
                    return target.AddYears((int)value * 10);
                case DateTimePeriod.None:
                default:
                    return target;
            }
        }

        /// <summary>
        /// Returns begin <see cref="System.DateTime" /> of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period type.</param>
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>Start of period date.</returns>
        public static DateTimeOffset GetStartOfPeriod(DateTimeOffset target, DateTimePeriod period, CultureInfo? cultureInfo = null)
            => Truncate(target, period, cultureInfo);

        /// <summary>
        /// Returns end <see cref="System.DateTime" /> of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period type.</param>
        /// <param name="cultureInfo">Specific culture to use. If null the current culture is used.</param>
        /// <returns>End of period date.</returns>
        public static DateTimeOffset GetEndOfPeriod(DateTimeOffset target, DateTimePeriod period, CultureInfo? cultureInfo = null)
            => Add(Truncate(target, period, cultureInfo), period, 1).AddMilliseconds(-1);

        /// <summary>
        /// Truncates date by specified period. Returns begin of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Type of truncation.</param>
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>Truncated date.</returns>
        public static DateTimeOffset Truncate(DateTimeOffset target, DateTimePeriod period, CultureInfo? cultureInfo = null)
        {
            // For reference: https://www.postgresql.org/docs/12/static/functions-datetime.html#FUNCTIONS-DATETIME-TRUNC
            switch (period)
            {
                case DateTimePeriod.Millisecond:
                    return new DateTimeOffset(target.Year, target.Month, target.Day, target.Hour, target.Minute, target.Second,
                        target.Millisecond, target.Offset);
                case DateTimePeriod.Second:
                    return new DateTimeOffset(target.Year, target.Month, target.Day, target.Hour, target.Minute, target.Second, target.Offset);
                case DateTimePeriod.Minute:
                    return new DateTimeOffset(target.Year, target.Month, target.Day, target.Hour, target.Minute, 0, target.Offset);
                case DateTimePeriod.Hour:
                    return new DateTimeOffset(target.Year, target.Month, target.Day, target.Hour, 0, 0, target.Offset);
                case DateTimePeriod.Day:
                    return new DateTimeOffset(target.Year, target.Month, target.Day, 0, 0, 0, target.Offset);
                case DateTimePeriod.Week:
                    var firstDayOfWeek = cultureInfo?.DateTimeFormat.FirstDayOfWeek ?? CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                    int diff = (7 + (target.DayOfWeek - firstDayOfWeek)) % 7;
                    return new DateTimeOffset(target.Year, target.Month, target.Day, 0, 0, 0, target.Offset).AddDays(-1 * diff);
                case DateTimePeriod.Month:
                    return new DateTimeOffset(target.Year, target.Month, 1, 0, 0, 0, target.Offset);
                case DateTimePeriod.Quarter:
                    return new DateTimeOffset(target.Year, target.Month, 1, 0, 0, 0, target.Offset).AddMonths(-(target.Month - 1) % 3);
                case DateTimePeriod.Year:
                    return new DateTimeOffset(target.Year, 1, 1, 0, 0, 0, target.Offset);
                case DateTimePeriod.Decade:
                    return new DateTimeOffset(target.Year - (target.Year % 10), 1, 1, 0, 0, 0, target.Offset);
                case DateTimePeriod.None:
                default:
                    return target;
            }
        }
    }
}
