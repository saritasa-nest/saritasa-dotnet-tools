// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;
using Xunit;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Message query tests.
    /// </summary>
    public class MessageQueryTests
    {
        [Fact]
        public void CreateFromString_TextQuery_ParsedQuery()
        {
            // Arrange
            var query = @"
id = 71e2614e-24f0-46b7-96c2-85568abeb67e
created > 2018-01-01 created<'2018-02-01 3:30:30'
contenttype = """"Saritasa.Test""""
errortype=Test
status=Rejected
type=COMMAND
duration>1000
skip 10 TAKE 1000";

            // Act
            var messageQuery = MessageQuery.CreateFromString(query);

            // Assert
            Assert.Equal(Guid.Parse("71e2614e-24f0-46b7-96c2-85568abeb67e"), messageQuery.Id);
            Assert.Equal(new DateTime(2018, 1, 1), messageQuery.CreatedStartDate);
            Assert.Equal(new DateTime(2018, 2, 1, 3, 30, 30), messageQuery.CreatedEndDate);
            Assert.Equal("\"Saritasa.Test\"", messageQuery.ContentType);
            Assert.Equal("Test", messageQuery.ErrorType);
            Assert.Equal(ProcessingStatus.Rejected, messageQuery.Status);
            Assert.Equal(MessageContextConstants.MessageTypeCommand, messageQuery.Type);
            Assert.Equal(1000, messageQuery.ExecutionDurationAbove);
            Assert.Null(messageQuery.ExecutionDurationBelow);
            Assert.Equal(10, messageQuery.Skip);
            Assert.Equal(1000, messageQuery.Take);
        }
    }
}
