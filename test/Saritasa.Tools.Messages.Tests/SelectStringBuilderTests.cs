// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Tests
{
    using System;
    using Xunit;
    using Common.Repositories.QueryProviders;

    public class SelectStringBuilderTests
    {
        [Fact]
        public void Test_sql_server_select_string_builder()
        {
            // Arrange
            var ssb = new SqlServerSelectStringBuilder();
            ssb.From("Table").Select("Column").Distinct()
                .Where("Column1").EqualsTo(1)
                .Where("Column2").Like("value")
                .GroupBy("Column3", "Column4");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT DISTINCT [Column] FROM [Table]
WHERE ([Column1] = 1) AND ([Column2] LIKE 'value')
GROUP BY [Column3], [Column4]";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Test_mysql_select_string_builder()
        {
            // Arrange
            var ssb = new MySqlSelectStringBuilder();
            ssb.From("Table").Select("Column").Distinct()
                .Where("Column1").EqualsTo(1)
                .Where("Column2").Like("value")
                .GroupBy("Column3", "Column4");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT DISTINCT `Column` FROM `Table`
WHERE (`Column1` = 1) AND (`Column2` LIKE 'value')
GROUP BY `Column3`, `Column4`";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Test_sqlite_select_string_builder()
        {
            // Arrange
            var ssb = new SqLiteSelectStringBuilder();
            ssb.From("Table").Select("Column").Distinct()
                .Where("Column1").EqualsTo(1)
                .Where("Column2").Like("value")
                .GroupBy("Column3", "Column4");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT DISTINCT Column FROM Table
WHERE (Column1 = 1) AND (Column2 LIKE 'value')
GROUP BY Column3, Column4";
            Assert.Equal(expectedResult, result);
        }
    }
}
