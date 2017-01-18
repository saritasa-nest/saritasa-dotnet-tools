// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Extensions
{
    using System;

    /// <summary>
    /// Helper methods for DateTime class.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Just checks is this a Saturday or Sunday.
        /// </summary>
        /// <param name="target">DateTime to check.</param>
        /// <returns>Is holiday.</returns>
        public static bool IsHoliday(this DateTime target)
        {
            return target.DayOfWeek == DayOfWeek.Saturday || target.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Truncates date. Begin of period.
        /// </summary>
        /// <param name="target">Target date.</param>
        /// <param name="period">Type of truncation.</param>
        /// <returns>Truncated date.</returns>
        public static DateTime Truncate(this DateTime target, DateTimePeriod period)
        {
            switch (period)
            {
                case DateTimePeriod.Seconds:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, 0, target.Kind);
                case DateTimePeriod.Minutes:
                    return new DateTime(target.Year, target.Month, target.Day, target.Hour, 0, 0, target.Kind);
                case DateTimePeriod.Hours:
                    return new DateTime(target.Year, target.Month, target.Day, 0, 0, 0, target.Kind);
                case DateTimePeriod.Days:
                    return new DateTime(target.Year, target.Month, 1, 0, 0, 0, target.Kind);
                case DateTimePeriod.Weeks:
                    return new DateTime(target.Year, target.Month, target.Day, 0, 0, 0, target.Kind)
                        .AddDays(-(int)target.DayOfWeek);
                case DateTimePeriod.Months:
                    return new DateTime(target.Year, 1, 1, 0, 0, 0, target.Kind);
                case DateTimePeriod.Quarters:
                    return new DateTime(target.Year, target.Month, 1, 0, 0, 0, target.Kind)
                        .AddMonths(-(target.Month - 1) % 3);
                case DateTimePeriod.Years:
                    return new DateTime(target.Year, 1, 1, 0, 0, 0, target.Kind);
                case DateTimePeriod.None:
                default:
                    return target;
            }
        }

        /// <summary>
        /// Is date between two startDate and endDate dates.
        /// </summary>
        /// <param name="date">Date to compare.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>True if date between startDate and endDate, False otherwise.</returns>
        public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate)
        {
            return date >= startDate && date <= endDate;
        }
    }
}
