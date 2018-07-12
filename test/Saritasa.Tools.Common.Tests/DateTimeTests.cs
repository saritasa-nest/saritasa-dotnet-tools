// Copyright (c) 2015-2018, Saritasa. All rights reserved.
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
        public void GetDiff_ThreePairOfDates_26Or50()
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
        public void CombineDateAndTime_DifferentKind_ThrowArgumentException()
        {
            // Arrange
            var dt1 = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var dt2 = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => { DateTimeUtils.CombineDateAndTime(dt1, dt2); });
        }

        [Fact]
        public void GetStartOfPeriod_USCulture_ReturnsSunday4()
        {
            // Arrange
            var dt = new DateTime(2017, 6, 5);

            // Act & Assert
            Assert.Equal(DayOfWeek.Sunday, DateTimeUtils.GetStartOfPeriod(dt, DateTimePeriod.Week, CultureInfo.InvariantCulture).DayOfWeek);
            Assert.Equal(4, DateTimeUtils.GetStartOfPeriod(dt, DateTimePeriod.Week, CultureInfo.InvariantCulture).Day);
        }

        [Fact]
        public void GetStartOfPeriod_RUCulture_ReturnsMonday5()
        {
            // Arrange
            var dt = new DateTime(2017, 6, 5);

            // Act & Assert
            Assert.Equal(DayOfWeek.Monday, DateTimeUtils.GetStartOfPeriod(dt, DateTimePeriod.Week, new CultureInfo("ru")).DayOfWeek);
            Assert.Equal(5, DateTimeUtils.GetStartOfPeriod(dt, DateTimePeriod.Week, new CultureInfo("ru")).Day);
        }

        [Fact]
        public void Truncate_USCulture_ReturnsJuly8()
        {
            // Arrange
            var date = new DateTime(2018, 7, 14, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = DateTimeUtils.Truncate(date, DateTimePeriod.Week, new CultureInfo("us"));

            // Assert
            Assert.Equal(8, result.Day);
        }

        [Fact]
        public void Truncate_RUCulture_ReturnsJuly9()
        {
            // Arrange
            var date = new DateTime(2018, 7, 15, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = DateTimeUtils.Truncate(date, DateTimePeriod.Week, new CultureInfo("ru"));

            // Assert
            Assert.Equal(9, result.Day);
        }

        [Fact]
        public void Truncate_CustomCulture_ReturnsJuly11()
        {
            // Arrange
            var date = new DateTime(2018, 7, 17, 0, 0, 0, DateTimeKind.Utc);
            var cultureInfo = new CultureInfo("us");
            cultureInfo.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Wednesday;

            // Act
            var result = DateTimeUtils.Truncate(date, DateTimePeriod.Week, cultureInfo);

            // Assert
            Assert.Equal(11, result.Day);
        }
    }
}
