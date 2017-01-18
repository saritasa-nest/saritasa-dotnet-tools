// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Extensions
{
    /// <summary>
    /// Date/time period truncation types.
    /// </summary>
    public enum DateTimePeriod
    {
        /// <summary>
        /// Do not trancate.
        /// </summary>
        None,

        /// <summary>
        /// Trancate by seconds: 2016-03-08 07:05:23 -> 2016-01-01 07:05:00.
        /// </summary>
        Seconds,

        /// <summary>
        /// Trancate by minutes: 2016-03-08 07:05:23 -> 2016-01-01 07:00:00.
        /// </summary>
        Minutes,

        /// <summary>
        /// Trancate by hours: 2016-03-08 07:05:23 -> 2016-01-01 00:00:00.
        /// </summary>
        Hours,

        /// <summary>
        /// Trancate by days: 2016-03-08 07:05:23 -> 2016-03-01 00:00:00.
        /// </summary>
        Days,

        /// <summary>
        /// Trancate by weeks: 2016-03-08 07:05:23 -> 2016-03-06 00:00:00. First day of week
        /// depends on current thread culture.
        /// </summary>
        Weeks,

        /// <summary>
        /// Trancate by months: 2016-03-08 07:05:23 -> 2016-03-01 00:00:00.
        /// </summary>
        Months,

        /// <summary>
        /// Trancate by quarters: 2016-03-08 07:05:23 -> 2016-01-01 00:00:00.
        /// </summary>
        Quarters,

        /// <summary>
        /// Trancate by years: 2016-03-08 07:05:23 -> 2016-01-01 00:00:00.
        /// </summary>
        Years,
    }
}
