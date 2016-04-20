//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading;
    using Xunit;
    using Interfaces;
    using Emails;

    /// <summary>
    /// Emails tests.
    /// </summary>
    public class EmailsTests
    {
        /// <summary>
        /// Interceptor for tests only. Has only counters.
        /// </summary>
        private class TestInterceptor : IEmailInterceptor
        {
            private int sendingCallCount, sentCallCount;

            public int SendingCallCount
            {
                get { return sendingCallCount; }
            }

            public int SentCallCount
            {
                get { return sentCallCount; }
            }

            public void Sending(MailMessage mailMessage, IDictionary<string, object> data, ref bool cancel)
            {
                Interlocked.Increment(ref sendingCallCount);
            }

            public void Sent(MailMessage mailMessage, IDictionary<string, object> data)
            {
                Interlocked.Increment(ref sentCallCount);
            }

            public void Reset()
            {
                sendingCallCount = 0;
                sentCallCount = 0;
            }
        }

        [Fact]
        public void TestThatInterceptorExecute()
        {
            EmailSender emailSender = new Dummy.DummyEmailSender();
            var testInterceptor = new TestInterceptor();
            emailSender.AddInterceptor(testInterceptor);

            emailSender.Send(new MailMessage()).Wait();
            emailSender.Send(new MailMessage()).Wait();
            emailSender.Send(new MailMessage()).Wait();

            Assert.Equal(3, testInterceptor.SendingCallCount);
            Assert.Equal(3, testInterceptor.SentCallCount);
        }

        [Fact]
        public void TestFilterEmailInterceptor()
        {
            EmailSender emailSender = new Dummy.DummyEmailSender();
            var filterInterceptor = new FilterEmailInterceptor();
            filterInterceptor.AddApprovedEmails("*@saritasa.com");
            emailSender.AddInterceptor(filterInterceptor);
            var testInterceptor = new TestInterceptor();
            emailSender.AddInterceptor(testInterceptor);

            emailSender.Send(new MailMessage("test@test.com", "test@example.com")).Wait();
            Assert.Equal(0, testInterceptor.SentCallCount);

            emailSender.Send(new MailMessage("test@test.com", "test@saritasa.com")).Wait();
            Assert.Equal(1, testInterceptor.SentCallCount);
        }
    }
}
