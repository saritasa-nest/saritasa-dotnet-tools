using System;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Common.ObjectSerializers;
using Saritasa.Tools.Messages.Common.Repositories.QueryProviders;
using Xunit;

namespace Saritasa.Tools.Messages.Tests
{
    public class SqlServerQueryProviderTests
    {
        [Fact]
        public void MyTestFact()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var ser = new JsonObjectSerializer();
            var provider = new SqlServerQueryProvider(ser);
            var query = MessageQuery.Create().WithId(guid).WithCreatedStartDate(DateTime.Now);

            // Act
            var result = provider.GetFilterScript(query);

            // Assert
            var expectedResult = $"SELECT * FROM [SaritasaMessages] WHERE ([Id] = '{guid}')";
            Assert.Equal(expectedResult, result);
        }
    }
}
