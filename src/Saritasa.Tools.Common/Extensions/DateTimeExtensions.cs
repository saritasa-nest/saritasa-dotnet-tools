// Copyright (c) 2015-2016, Saritasa. All rights reserved.
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
        /// Returns begin of month for specified date.
        /// </summary>
        /// <param name="target">The date to get year and month.</param>
        /// <returns>Begin of month date.</returns>
        public static DateTime BeginOfMonth(this DateTime target)
        {
            return new DateTime(target.Year, target.Month, 1);
        }

        /// <summary>
        /// Returns end of month for specified date.
        /// </summary>
        /// <param name="target">The date to get year and month.</param>
        /// <returns>End of month date.</returns>
        public static DateTime EndOfMonth(this DateTime target)
        {
            return target.AddMonths(1).AddMilliseconds(-1);
        }

        /// <summary>
        /// Date trancation types.
        /// </summary>
        public enum DateTimeTruncation
        {
            /// <summary>
            /// Do not trancate.
            /// </summary>
            None,

            /// <summary>
            /// Trancate by seconds: 2016-03-06 07:05:23 -> 2016-01-01 07:05:00.
            /// </summary>
            Seconds,

            /// <summary>
            /// Trancate by minutes: 2016-03-06 07:05:23 -> 2016-01-01 07:00:00.
            /// </summary>
            Minutes,

            /// <summary>
            /// Trancate by hours: 2016-03-06 07:05:23 -> 2016-01-01 00:00:00.
            /// </summary>
            Hours,

            /// <summary>
            /// Trancate by days: 2016-03-06 07:05:23 -> 2016-03-00 00:00:00.
            /// </summary>
            Days,

            /// <summary>
            /// Trancate by months: 2016-03-06 07:05:23 -> 2016-00-00 00:00:00.
            /// </summary>
            Months
        }

        /// <summary>
        /// Truncates date.
        /// </summary>
        /// <param name="date">Target date/</param>
        /// <param name="truncation">Type of truncation.</param>
        /// <returns>Truncated date.</returns>
        public static DateTime Truncate(this DateTime date, DateTimeTruncation truncation)
        {
            switch (truncation)
            {
                case DateTimeTruncation.None:
                    return date;
                case DateTimeTruncation.Seconds:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, date.Kind);
                case DateTimeTruncation.Minutes:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, date.Kind);
                case DateTimeTruncation.Hours:
                    return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, date.Kind);
                case DateTimeTruncation.Days:
                    return new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
                case DateTimeTruncation.Months:
                    return new DateTime(date.Year, 1, 1, 0, 0, 0, date.Kind);
                default:
                    return date;
            }
        }
    }
}
