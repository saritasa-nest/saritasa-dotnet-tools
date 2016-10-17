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
    using Emails.Interceptors;

    /// <summary>
    /// Emails tests.
    /// </summary>
    [TestFixture]
    public class EmailsTests
    {
        [Test]
        public void Test_that_interceptor_executes()
        {
            var emailSender = new DummyEmailSender();
            var countEmailsInterceptor = new CountEmailsInterceptor();
            emailSender.AddInterceptor(countEmailsInterceptor);

            emailSender.SendAsync(new MailMessage()).Wait();
            emailSender.SendAsync(new MailMessage()).Wait();
            emailSender.SendAsync(new MailMessage()).Wait();

            Assert.That(countEmailsInterceptor.SendingCallCount, Is.EqualTo(3));
            Assert.That(countEmailsInterceptor.SentCallCount, Is.EqualTo(3));
        }

        [Test]
        public void Email_filter_should_filter_addresses()
        {
            var emailSender = new DummyEmailSender();
            var filterInterceptor = new FilterEmailInterceptor("*@saritasa.com; *@saritasa-hosting.com");
            emailSender.AddInterceptor(filterInterceptor);
            var countEmailsInterceptor = new CountEmailsInterceptor();
            emailSender.AddInterceptor(countEmailsInterceptor);

            emailSender.SendAsync(new MailMessage("test@test.com", "test@example.com")).Wait();
            Assert.That(countEmailsInterceptor.SentCallCount, Is.EqualTo(0));

            emailSender.SendAsync(new MailMessage("test@test.com", "test@saritasa.com")).Wait();
            Assert.That(countEmailsInterceptor.SentCallCount, Is.EqualTo(1));

            emailSender.SendAsync(new MailMessage("test@test.com", "test@saritasa-hosting.com")).Wait();
            Assert.That(countEmailsInterceptor.SentCallCount, Is.EqualTo(2));

            filterInterceptor.SetApprovedEmails("*");
            emailSender.SendAsync(new MailMessage("test@test.com", "test@example.com")).Wait();
            Assert.That(countEmailsInterceptor.SentCallCount, Is.EqualTo(3));
        }
    }
}
