// Copyright (c) 2015-2019, Saritasa. All rights reserved.
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
        public void Build_SqlWithColumn1Column2Value_MatchSqlServerSelectString()
        {
            // Arrange
            var ssb = new SqlServerSelectStringBuilder();
            ssb.SelectAll().From("Table")
                .Where("Column1").EqualsTo(1)
                .Where("Column2").Like("value");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT * FROM [Table]
WHERE ([Column1] = 1) AND ([Column2] LIKE 'value')";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Build_SqlWithColumn1Column2Value_MatchMySqlSelectString()
        {
            // Arrange
            var ssb = new MySqlSelectStringBuilder();
            ssb.SelectAll().From("Table")
                .Where("Column1").EqualsTo(1)
                .Where("Column2").Like("value");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT * FROM `Table`
WHERE (`Column1` = 1) AND (`Column2` LIKE 'value')";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Build_SqlWithColumn1Column2Value_MatchSQLiteSelectString()
        {
            // Arrange
            var ssb = new SqLiteSelectStringBuilder();
            ssb.SelectAll().From("Table")
                .Where("Column1").EqualsTo(1)
                .Where("Column2").Like("value");

            // Act
            var result = ssb.Build();

            // Assert
            var expectedResult =
@"SELECT * FROM Table
WHERE (Column1 = 1) AND (Column2 LIKE 'value')";
            Assert.Equal(expectedResult, result);
        }
    }
}
