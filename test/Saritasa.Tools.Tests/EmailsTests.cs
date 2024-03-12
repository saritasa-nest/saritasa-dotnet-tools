// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if !NETSTANDARD1_6
using System.Net.Mail;
#endif
using Xunit;
using Saritasa.Tools.Emails;
using Saritasa.Tools.Emails.Interceptors;
using System.Threading.Tasks;
#pragma warning disable CS1591

namespace Saritasa.Tools.Tests;

/// <summary>
/// Emails tests.
/// </summary>
public class EmailsTests
{
    private readonly EmailSender emailSender = new DummyEmailSender();
    private readonly CountEmailsInterceptor countEmailsInterceptor = new CountEmailsInterceptor();

    public EmailsTests()
    {
        emailSender.AddInterceptor(countEmailsInterceptor);
    }

    [Fact]
    public async Task SendAsync_Send3Times_CountEmailsInterceptorShouldBe3()
    {
        // Act
        await emailSender.SendAsync(new MailMessage());
        await emailSender.SendAsync(new MailMessage());
        await emailSender.SendAsync(new MailMessage());

        // Assert
        Assert.Equal(3, countEmailsInterceptor.SendingCallCount);
        Assert.Equal(3, countEmailsInterceptor.SentCallCount);
    }

    [Theory]
    [InlineData("*@saritasa.com; *@saritasa-hosting.com", "test@saritasa.com")]
    [InlineData("*@example.com", "test+test@example.com")]
    [InlineData("wrong *@example.com", "test+test@example.com")]
    [InlineData("test+?@example.com", "test+3@example.com")]
    public async Task SendAsyncWithFilter_ApprovedEmailPatternsInterceptorWithWildcard_ShouldSend(string pattern, string email)
    {
        // Arrange
        var filterInterceptor = new ApprovedEmailPatternsInterceptor(pattern);
        emailSender.AddInterceptor(filterInterceptor);

        // Act
        await emailSender.SendAsync(new MailMessage("test@saritasa.com", email));

        // Assert
        Assert.Equal(1, countEmailsInterceptor.SentCallCount);
    }

    [Theory]
    [InlineData("*@saritasa.com; *@saritasa-hosting.com", "test@example.com")]
    [InlineData("test+?@example.com", "test+33@example.com")]
    public async Task SendAsyncWithFilter_ApprovedEmailPatternsInterceptorWithWildcard_ShouldNotSend(string pattern, string email)
    {
        // Arrange
        var filterInterceptor = new ApprovedEmailPatternsInterceptor(pattern);
        emailSender.AddInterceptor(filterInterceptor);

        // Act
        await emailSender.SendAsync(new MailMessage("test@saritasa.com", email));

        // Assert
        Assert.Equal(0, countEmailsInterceptor.SentCallCount);
    }

    [Theory]
    [InlineData("*@saritasa-hosting.com;", "test@saritasa.com")]
    public async Task SendAsyncWithFilter_DeniedEmailPatternsInterceptorWithWildcard_ShouldSend(string pattern, string email)
    {
        // Arrange
        var filterInterceptor = new DeniedEmailPatternsInterceptor(pattern);
        emailSender.AddInterceptor(filterInterceptor);

        // Act
        await emailSender.SendAsync(new MailMessage("test@saritasa.com", email));

        // Assert
        Assert.Equal(1, countEmailsInterceptor.SentCallCount);
    }
}
