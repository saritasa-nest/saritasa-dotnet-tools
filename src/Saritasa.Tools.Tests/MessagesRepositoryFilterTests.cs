// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Text.RegularExpressions;
    using NUnit.Framework;
    using Messages;
    using Messages.PipelineMiddlewares;

    [TestFixture]
    public class MessagesRepositoryFilterTests
    {
        [Test]
        public void Repository_should_filter_by_status()
        {
            var filter = RepositoryMessagesFilter.Create().WithStatus(Message.ProcessingStatus.Completed);
            var msg1 = new Message()
            {
                Status = Message.ProcessingStatus.Failed,
            };
            Assert.That(filter.IsMatch(msg1), Is.False);
            var msg2 = new Message()
            {
                Status = Message.ProcessingStatus.Completed,
            };
            Assert.That(filter.IsMatch(msg2), Is.True);
            filter = filter.WithStatus(Message.ProcessingStatus.Failed);
            Assert.That(filter.IsMatch(msg1), Is.True);
        }

        [Test]
        public void Repository_should_filter_by_execution_duration()
        {
            var filter = RepositoryMessagesFilter.Create().WithExecutionDurationAbove(100);
            var msg = new Message()
            {
                ExecutionDuration = 200,
            };
            Assert.That(filter.IsMatch(msg), Is.True);
            msg.ExecutionDuration = 99;
            Assert.That(filter.IsMatch(msg), Is.False);
        }

        [Test]
        public void Repository_should_filter_by_content_type()
        {
            var filter = RepositoryMessagesFilter.Create().WithExcludeContentType(new Regex(@".(C|c)orrectCommand$"));
            var msg = new Message()
            {
                ContentType = "Saritasa.Demo.IncorrectCommand",
            };
            Assert.That(filter.IsMatch(msg), Is.False);
            msg.ContentType = "Saritasa.Demo.CorrectCommand";
            Assert.That(filter.IsMatch(msg), Is.True);
        }
    }
}
