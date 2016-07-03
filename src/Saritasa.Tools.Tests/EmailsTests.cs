// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading;
    using NUnit.Framework;
    using Emails;
    using Emails.SystemMail;

    /// <summary>
    /// Emails tests.
    /// </summary>
    [TestFixture]
    public class EmailsTests
    {
        /// <summary>
        /// Interceptor for tests only. Has only counters.
        /// </summary>
        private class TestInterceptor : IEmailInterceptor<MailMessage>
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

        [Test]
        public void Test_that_interceptor_executes()
        {
            var emailSender = new Dummy.DummyEmailSender();
            var testInterceptor = new TestInterceptor();
            emailSender.AddInterceptor(testInterceptor);

            emailSender.Send(new MailMessage()).Wait();
            emailSender.Send(new MailMessage()).Wait();
            emailSender.Send(new MailMessage()).Wait();

            Assert.That(testInterceptor.SendingCallCount, Is.EqualTo(3));
            Assert.That(testInterceptor.SentCallCount, Is.EqualTo(3));
        }

        [Test]
        public void Email_filter_should_filter_addresses()
        {
            var emailSender = new Dummy.DummyEmailSender();
            var filterInterceptor = new SystemMailFilterEmailInterceptor("*@saritasa.com; *@saritasa-hosting.com");
            emailSender.AddInterceptor(filterInterceptor);
            var testInterceptor = new TestInterceptor();
            emailSender.AddInterceptor(testInterceptor);

            emailSender.Send(new MailMessage("test@test.com", "test@example.com")).Wait();
            Assert.That(testInterceptor.SentCallCount, Is.EqualTo(0));

            emailSender.Send(new MailMessage("test@test.com", "test@saritasa.com")).Wait();
            Assert.That(testInterceptor.SentCallCount, Is.EqualTo(1));

            emailSender.Send(new MailMessage("test@test.com", "test@saritasa-hosting.com")).Wait();
            Assert.That(testInterceptor.SentCallCount, Is.EqualTo(2));

            filterInterceptor.SetApprovedEmails("*");
            emailSender.Send(new MailMessage("test@test.com", "test@example.com")).Wait();
            Assert.That(testInterceptor.SentCallCount, Is.EqualTo(3));
        }
    }
}
