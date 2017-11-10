// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;
using Xunit;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Common.PipelineMiddlewares;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Message repository filter tests.
    /// </summary>
    public class MessagesRepositoryFilterTests
    {
        [Fact]
        public void Repository_should_filter_by_status()
        {
            // Arrange
            var filter = RepositoryMessagesFilter.Create().WithStatus(ProcessingStatus.Completed);
            var msg1 = new MessageRecord
            {
                Status = ProcessingStatus.Failed,
            };

            // Act & assert
            Assert.False(filter.IsMatch(msg1));
            var msg2 = new MessageRecord
            {
                Status = ProcessingStatus.Completed,
            };
            Assert.True(filter.IsMatch(msg2));
            filter = filter.WithStatus(ProcessingStatus.Failed);
            Assert.True(filter.IsMatch(msg1));
        }

        [Fact]
        public void Repository_should_filter_by_execution_duration()
        {
            var filter = RepositoryMessagesFilter.Create().WithExecutionDurationAbove(100);
            var msg = new MessageRecord
            {
                ExecutionDuration = 200,
            };
            Assert.True(filter.IsMatch(msg));
            msg.ExecutionDuration = 99;
            Assert.False(filter.IsMatch(msg));
        }

        [Fact]
        public void Repository_should_filter_by_content_type()
        {
            var filter = RepositoryMessagesFilter.Create()
                .WithIncludeContentType(new Regex(@".(C|c)orrectCommand$"))
                .WithExcludeContentType(new Regex(@".IncorrectCommand$"));
            var msg = new MessageRecord
            {
                ContentType = "Saritasa.Demo.IncorrectCommand",
            };
            Assert.False(filter.IsMatch(msg));

            msg.ContentType = "Saritasa.Demo.CorrectCommand";
            Assert.True(filter.IsMatch(msg));

            filter = filter.WithExcludeContentType(new Regex(@"Command"));
            Assert.False(filter.IsMatch(msg));
        }

        [Fact]
        public void Repository_should_filter_by_type()
        {
            var filter = RepositoryMessagesFilter.Create().WithType(MessageContextConstants.MessageTypeCommand);
            var msgquery = new MessageRecord
            {
                Type = MessageContextConstants.MessageTypeQuery,
            };
            Assert.False(filter.IsMatch(msgquery));

            filter = filter.WithType(MessageContextConstants.MessageTypeQuery);
            Assert.True(filter.IsMatch(msgquery));
        }
    }
}
