// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Xunit;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// Date time tests.
    /// </summary>
    public class DateTimeTests
    {
        [Fact]
        public void Begin_of_month_should_truncate_date()
        {
            // Arrange
            var date1 = new DateTime(2017, 2, 3);
            var date2 = new DateTime(2016, 6, 3);
            var date3 = new DateTime(2017, 9, 30);
            var date4 = new DateTime(2017, 3, 1);

            // Act & Assert
            Assert.Equal(new DateTime(2017, 2, 1), DateTimeUtils.GetStartOfPeriod(date1, DateTimePeriod.Month));
            Assert.Equal(new DateTime(2016, 6, 1), DateTimeUtils.GetStartOfPeriod(date2, DateTimePeriod.Month));
            Assert.Equal(new DateTime(2017, 9, 1), DateTimeUtils.GetStartOfPeriod(date3, DateTimePeriod.Month));
            Assert.Equal(new DateTime(2017, 3, 1), DateTimeUtils.GetStartOfPeriod(date4, DateTimePeriod.Month));
        }

        [Fact]
        public void End_of_month_should_truncate_date()
        {
            // Arrange
            var date1 = new DateTime(2017, 2, 3);
            var date2 = new DateTime(2016, 6, 3);
            var date3 = new DateTime(2017, 9, 30);
            var date4 = new DateTime(2017, 3, 1);

            // Act & Assert
            Assert.Equal(new DateTime(2017, 2, 28), DateTimeUtils.GetEndOfPeriod(date1, DateTimePeriod.Month).Date);
            Assert.Equal(new DateTime(2016, 6, 30), DateTimeUtils.GetEndOfPeriod(date2, DateTimePeriod.Month).Date);
            Assert.Equal(new DateTime(2017, 9, 30), DateTimeUtils.GetEndOfPeriod(date3, DateTimePeriod.Month).Date);
            Assert.Equal(new DateTime(2017, 3, 31), DateTimeUtils.GetEndOfPeriod(date4, DateTimePeriod.Month).Date);
        }

        [Fact]
        public void Start_of_quarters_should_be_correct()
        {
            // Arrange
            var q1 = new DateTime(2017, 2, 3);
            var q2 = new DateTime(2017, 5, 3);
            var q3 = new DateTime(2017, 7, 5);
            var q4 = new DateTime(2017, 11, 3);

            // Act & Assert
            Assert.Equal(new DateTime(2017, 1, 1), DateTimeUtils.GetStartOfPeriod(q1, DateTimePeriod.Quarter));
            Assert.Equal(new DateTime(2017, 4, 1), DateTimeUtils.GetStartOfPeriod(q2, DateTimePeriod.Quarter));
            Assert.Equal(new DateTime(2017, 7, 1), DateTimeUtils.GetStartOfPeriod(q3, DateTimePeriod.Quarter));
            Assert.Equal(new DateTime(2017, 10, 1), DateTimeUtils.GetStartOfPeriod(q4, DateTimePeriod.Quarter));
        }

        [Fact]
        public void Diff_for_months_should_be_correct()
        {
            // Arrange
            var diffa1 = new DateTime(2016, 10, 12);
            var diffa2 = new DateTime(2014, 8, 1);
            var diffb1 = new DateTime(2018, 10, 22);
            var diffb2 = new DateTime(2014, 8, 10);

            // Act & Assert
            Assert.InRange(DateTimeUtils.GetDiff(diffa1, diffa2, DateTimePeriod.Month), 26.35, 26.355);
            Assert.InRange(DateTimeUtils.GetDiff(diffa2, diffa1, DateTimePeriod.Month), 26.35, 26.355);
            Assert.InRange(DateTimeUtils.GetDiff(diffb2, diffb1, DateTimePeriod.Month), 50.38, 50.388);
        }

        [Fact]
        public void Combine_date_and_time_with_different_kinds_should_generate_exception()
        {
            // Arrange
            var dt1 = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var dt2 = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => { DateTimeUtils.CombineDateAndTime(dt1, dt2); });
        }

        [Fact]
        public void Start_of_week_should_be_culture_correct()
        {
            // Arrange
            var dt = new DateTime(2017, 6, 5);

            // Act & Assert
            Assert.Equal(DayOfWeek.Sunday, DateTimeUtils.GetStartOfPeriod(dt, DateTimePeriod.Week, CultureInfo.InvariantCulture).DayOfWeek);
            Assert.Equal(DayOfWeek.Monday, DateTimeUtils.GetStartOfPeriod(dt, DateTimePeriod.Week, new CultureInfo("ru")).DayOfWeek);
        }
    }
}
