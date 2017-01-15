// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Tests
{
    using System;
    using Xunit;
    using Common;

    /// <summary>
    /// Message queries tests.
    /// </summary>
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
