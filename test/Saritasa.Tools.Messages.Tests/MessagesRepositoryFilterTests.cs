// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Tests
{
    using System;
    using System.Text.RegularExpressions;
    using Xunit;
    using Messages.Common.PipelineMiddlewares;
    using Messages.Common;

    public class MessagesRepositoryFilterTests
    {
        [Fact]
        public void Repository_should_filter_by_status()
        {
            var filter = RepositoryMessagesFilter.Create().WithStatus(Message.ProcessingStatus.Completed);
            var msg1 = new Message()
            {
                Status = Message.ProcessingStatus.Failed,
            };
            Assert.False(filter.IsMatch(msg1));
            var msg2 = new Message()
            {
                Status = Message.ProcessingStatus.Completed,
            };
            Assert.True(filter.IsMatch(msg2));
            filter = filter.WithStatus(Message.ProcessingStatus.Failed);
            Assert.True(filter.IsMatch(msg1));
        }

        [Fact]
        public void Repository_should_filter_by_execution_duration()
        {
            var filter = RepositoryMessagesFilter.Create().WithExecutionDurationAbove(100);
            var msg = new Message()
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
            var msg = new Message()
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
            var filter = RepositoryMessagesFilter.Create().WithType(Message.MessageTypeCommand);
            var msgquery = new Message()
            {
                Type = Message.MessageTypeQuery,
            };
            Assert.False(filter.IsMatch(msgquery));

            filter = filter.WithType(Message.MessageTypeQuery);
            Assert.True(filter.IsMatch(msgquery));
        }
    }
}
