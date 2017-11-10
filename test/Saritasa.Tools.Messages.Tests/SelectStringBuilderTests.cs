// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Xunit;
using Saritasa.Tools.Messages.Common.Repositories.QueryProviders;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Select string builder tests.
    /// </summary>
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

        [Fact]
        public void Test_sql_server_select_string_builder_join_clause()
        {
            // Arrange
            var ssb = new SqlServerSelectStringBuilder();
            ssb.Select("Column1", "Column2").From("Table1")
                .Join("Table2").On("Column2").EqualsTo("Table1", "Column1")
                .LeftJoin("Table3").On("Column3").EqualsTo("Table2", "Column2");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT [Column1], [Column2] FROM [Table1]
INNER JOIN [Table2] ON [Table2].[Column2] = [Table1].[Column1]
LEFT JOIN [Table3] ON [Table3].[Column3] = [Table2].[Column2]";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Test_mysql_select_string_builder_join_clause()
        {
            // Arrange
            var ssb = new MySqlSelectStringBuilder();
            ssb.Select("Column1", "Column2").From("Table1")
                .Join("Table2").On("Column2").EqualsTo("Table1", "Column1")
                .LeftJoin("Table3").On("Column3").EqualsTo("Table2", "Column2");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT `Column1`, `Column2` FROM `Table1`
INNER JOIN `Table2` ON `Table2`.`Column2` = `Table1`.`Column1`
LEFT JOIN `Table3` ON `Table3`.`Column3` = `Table2`.`Column2`";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Test_sqlite_select_string_builder_join_clause()
        {
            // Arrange
            var ssb = new SqLiteSelectStringBuilder();
            ssb.Select("Column1", "Column2").From("Table1")
                .Join("Table2").On("Column2").EqualsTo("Table1", "Column1")
                .LeftJoin("Table3").On("Column3").EqualsTo("Table2", "Column2");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT Column1, Column2 FROM Table1
INNER JOIN Table2 ON Table2.Column2 = Table1.Column1
LEFT OUTER JOIN Table3 ON Table3.Column3 = Table2.Column2";
            Assert.Equal(expectedResult, result);
        }
    }
}
