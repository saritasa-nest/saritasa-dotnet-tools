using System;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Common.ObjectSerializers;
using Saritasa.Tools.Messages.Common.Repositories.QueryProviders;
using Xunit;

namespace Saritasa.Tools.Messages.Tests
{
    public class QueryProviderTests
    {
        [Fact]
        public void Sql_server_query_provider_get_filter_test()
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
        public void Mysql_query_provider_get_filter_test()
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
        public void Sqlite_query_provider_get_filter_test()
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
    }
}
