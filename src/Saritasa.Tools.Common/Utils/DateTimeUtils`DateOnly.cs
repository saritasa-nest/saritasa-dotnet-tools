// Copyright (c) 2015-2022, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Saritasa.Tools.Common.Utils
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// Date time utils. Overloads for <see cref="DateOnly" />.
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
        public static IEnumerable<DateTime> GetRange(DateOnly fromDate, DateOnly toDate, DateTimePeriod period = DateTimePeriod.Day)
            => GetRange(fromDate.ToDateTime(TimeOnly.MinValue), toDate.ToDateTime(TimeOnly.MinValue), period);

        /// <summary>
        /// Add to target date a specific amount of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Period to add.</param>
        /// <param name="value">The specific amount to add.</param>
        /// <returns>New modified date value.</returns>
        public static DateTime Add(DateOnly target, DateTimePeriod period, double value)
        {
            var dateTime = target.ToDateTime(TimeOnly.MinValue);
            switch (period)
            {
                case DateTimePeriod.Millisecond:
                    return dateTime.AddMilliseconds(value);
                case DateTimePeriod.Second:
                    return dateTime.AddSeconds(value);
                case DateTimePeriod.Minute:
                    return dateTime.AddMinutes(value);
                case DateTimePeriod.Hour:
                    return dateTime.AddHours(value);
                case DateTimePeriod.Day:
                    return dateTime.AddDays(value);
                case DateTimePeriod.Week:
                    return dateTime.AddDays(value * 7);
                case DateTimePeriod.Month:
                    return dateTime.AddMonths((int)value);
                case DateTimePeriod.Quarter:
                    return dateTime.AddMonths((int)value * 4);
                case DateTimePeriod.Year:
                    return dateTime.AddYears((int)value);
                case DateTimePeriod.Decade:
                    return dateTime.AddYears((int)value * 10);
                case DateTimePeriod.None:
                default:
                    return dateTime;
            }
        }

        /// <summary>
        /// Truncates date by specified period. Returns begin of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Type of truncation.</param>
        /// <param name="cultureInfo">Specific culture to use. If null current culture is used.</param>
        /// <returns>Truncated date.</returns>
        public static DateOnly Truncate(DateOnly target, DateTimePeriod period, CultureInfo? cultureInfo = null)
        {
            // For reference: https://www.postgresql.org/docs/12/static/functions-datetime.html#FUNCTIONS-DATETIME-TRUNC
            switch (period)
            {
                case DateTimePeriod.Month:
                    return new DateOnly(target.Year, target.Month, 1);
                case DateTimePeriod.Quarter:
                    return new DateOnly(target.Year, target.Month, 1).AddMonths(-(target.Month - 1) % 3);
                case DateTimePeriod.Year:
                    return new DateOnly(target.Year, 1, 1);
                case DateTimePeriod.Decade:
                    return new DateOnly(target.Year - (target.Year % 10), 1, 1);
                case DateTimePeriod.None:
                default:
                    return target;
            }
        }
    }
#endif
}
