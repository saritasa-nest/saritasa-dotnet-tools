// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Net.Mail;
    using Xunit;
    using Emails;
    using Emails.Interceptors;

    /// <summary>
    /// Emails tests.
    /// </summary>
    public class EmailsTests
    {
        [Fact]
        public void Test_that_interceptor_executes()
        {
            var emailSender = new DummyEmailSender();
            var countEmailsInterceptor = new CountEmailsInterceptor();
            emailSender.AddInterceptor(countEmailsInterceptor);

            emailSender.SendAsync(new MailMessage()).Wait();
            emailSender.SendAsync(new MailMessage()).Wait();
            emailSender.SendAsync(new MailMessage()).Wait();

            Assert.Equal(3, countEmailsInterceptor.SendingCallCount);
            Assert.Equal(3, countEmailsInterceptor.SentCallCount);
        }

        [Fact]
        public void Email_filter_should_filter_addresses()
        {
            var emailSender = new DummyEmailSender();
            var filterInterceptor = new FilterEmailInterceptor("*@saritasa.com; *@saritasa-hosting.com");
            emailSender.AddInterceptor(filterInterceptor);
            var countEmailsInterceptor = new CountEmailsInterceptor();
            emailSender.AddInterceptor(countEmailsInterceptor);

            emailSender.SendAsync(new MailMessage("test@test.com", "test@example.com")).Wait();
            Assert.Equal(0, countEmailsInterceptor.SentCallCount);

            emailSender.SendAsync(new MailMessage("test@test.com", "test@saritasa.com")).Wait();
            Assert.Equal(1, countEmailsInterceptor.SentCallCount);

            emailSender.SendAsync(new MailMessage("test@test.com", "test@saritasa-hosting.com")).Wait();
            Assert.Equal(2, countEmailsInterceptor.SentCallCount);

            filterInterceptor.SetApprovedEmails("*");
            emailSender.SendAsync(new MailMessage("test@test.com", "test@example.com")).Wait();
            Assert.Equal(3, countEmailsInterceptor.SentCallCount);
        }
    }
}
