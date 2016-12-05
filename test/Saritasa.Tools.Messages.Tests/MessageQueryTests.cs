using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Common;
using Xunit;

namespace Saritasa.Tools.Messages.Tests
{
    public class MessageQueryTests
    {
        [Fact]
        public void MessageQuery_should_filter_by_Id()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var query = MessageQuery.Create()
                .WithId(guid);
            var expectedMessage = new Message { Id = guid };

            // Act
            var result = query.Match(expectedMessage);

            // Assert
            Assert.True(result);
        }
    }
}
