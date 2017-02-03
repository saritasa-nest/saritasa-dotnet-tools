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
        /// Period not specified.
        /// </summary>
        None,

        /// <summary>
        /// Second. Trancation example: 2016-03-08 07:05:23 -> 2016-01-01 07:05:00.
        /// </summary>
        Second,

        /// <summary>
        /// Minute. Trancation example: 2016-03-08 07:05:23 -> 2016-01-01 07:00:00.
        /// </summary>
        Minute,

        /// <summary>
        /// Hour. Trancation example: 2016-03-08 07:05:23 -> 2016-01-01 00:00:00.
        /// </summary>
        Hour,

        /// <summary>
        /// Day. Trancation example: 2016-03-08 07:05:23 -> 2016-03-01 00:00:00.
        /// </summary>
        Day,

        /// <summary>
        /// Week. Trancation example: 2016-03-08 07:05:23 -> 2016-03-06 00:00:00. First day of week
        /// depends on current thread culture.
        /// </summary>
        Week,

        /// <summary>
        /// Month. Trancation example: 2016-03-08 07:05:23 -> 2016-03-01 00:00:00.
        /// </summary>
        Month,

        /// <summary>
        /// Quarter. Trancation example: 2016-03-08 07:05:23 -> 2016-01-01 00:00:00.
        /// </summary>
        Quarter,

        /// <summary>
        /// Year. Trancation example: 2016-03-08 07:05:23 -> 2016-01-01 00:00:00.
        /// </summary>
        Year,
    }
}
