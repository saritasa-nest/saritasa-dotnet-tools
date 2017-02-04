// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Tests
{
    using System;
    using Xunit;
    using Common;
    using Common.ObjectSerializers;
    using Common.Repositories.QueryProviders;

    /// <summary>
    /// Query provider tests.
    /// </summary>
    public class QueryProviderTests
    {
        [Fact]
        public void Sql_server_query_provider_get_filter_by_id_test()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var ser = new JsonObjectSerializer();
            var provider = new SqlServerQueryProvider(ser);
            var query = MessageQuery.Create().WithId(guid);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult = 
$@"SELECT TOP 1000 * FROM [SaritasaMessages]
WHERE ([ContentId] = '{guid}')";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Sql_server_query_provider_get_filter_by_date_test()
        {
            // Arrange
            var startDate = new DateTime(2016, 1, 1);
            var endDate = new DateTime(2016, 12, 31, 23, 0, 0);
            var ser = new JsonObjectSerializer();
            var provider = new SqlServerQueryProvider(ser);
            var query = MessageQuery.Create()
                .WithCreatedStartDate(startDate)
                .WithCreatedEndDate(endDate);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult =
$@"SELECT TOP 1000 * FROM [SaritasaMessages]
WHERE ([CreatedAt] >= '{startDate:yyyy-MM-dd hh:mm:ss}') AND ([CreatedAt] <= '{endDate:yyyy-MM-dd hh:mm:ss}')";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Sql_server_query_provider_get_filter_range_test()
        {
            // Arrange
            var ser = new JsonObjectSerializer();
            var provider = new SqlServerQueryProvider(ser);
            var query = MessageQuery.Create().WithRange(20, 10);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult =
$@"SELECT * FROM [SaritasaMessages]
ORDER BY [ContentId]
OFFSET 20 ROWS
FETCH NEXT 10 ROWS ONLY";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Mysql_query_provider_get_filter_by_id_test()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var ser = new JsonObjectSerializer();
            var provider = new MySqlQueryProvider(ser);
            var query = MessageQuery.Create().WithId(guid);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult = 
$@"SELECT * FROM `saritasa_messages`
WHERE (`content_id` = '{guid}')
LIMIT 1000";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Mysql_query_provider_get_filter_by_date_test()
        {
            // Arrange
            var startDate = new DateTime(2016, 1, 1);
            var endDate = new DateTime(2016, 12, 31, 23, 0, 0);
            var ser = new JsonObjectSerializer();
            var provider = new MySqlQueryProvider(ser);
            var query = MessageQuery.Create()
                .WithCreatedStartDate(startDate)
                .WithCreatedEndDate(endDate);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult =
$@"SELECT * FROM `saritasa_messages`
WHERE (`created_at` >= '{startDate:yyyy-MM-dd hh:mm:ss}') AND (`created_at` <= '{endDate:yyyy-MM-dd hh:mm:ss}')
LIMIT 1000";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Mysql_query_provider_get_filter_range_test()
        {
            // Arrange
            var ser = new JsonObjectSerializer();
            var provider = new MySqlQueryProvider(ser);
            var query = MessageQuery.Create().WithRange(20, 10);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult =
$@"SELECT * FROM `saritasa_messages`
LIMIT 20, 10";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Sqlite_query_provider_get_filter_by_id_test()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var ser = new JsonObjectSerializer();
            var provider = new SqliteQueryProvider(ser);
            var query = MessageQuery.Create().WithId(guid);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult = 
$@"SELECT * FROM saritasa_messages
WHERE (content_id = '{guid}')
LIMIT 1000";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Sqlite_query_provider_get_filter_by_date_test()
        {
            // Arrange
            var startDate = new DateTime(2016, 1, 1);
            var endDate = new DateTime(2016, 12, 31, 23, 0, 0);
            var ser = new JsonObjectSerializer();
            var provider = new SqliteQueryProvider(ser);
            var query = MessageQuery.Create()
                .WithCreatedStartDate(startDate)
                .WithCreatedEndDate(endDate);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult =
$@"SELECT * FROM saritasa_messages
WHERE (created_at >= '{startDate:yyyy-MM-dd hh:mm:ss}') AND (created_at <= '{endDate:yyyy-MM-dd hh:mm:ss}')
LIMIT 1000";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Sqlite_query_provider_get_filter_range_test()
        {
            // Arrange
            var ser = new JsonObjectSerializer();
            var provider = new SqliteQueryProvider(ser);
            var query = MessageQuery.Create().WithRange(20, 10);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult =
$@"SELECT * FROM saritasa_messages
LIMIT 10 OFFSET 20";
            Assert.Equal(expectedResult, result);
        }
    }
}
