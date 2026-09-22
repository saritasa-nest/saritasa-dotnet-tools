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
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);
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

    [Fact]
    public async Task SmtpClientEmailSender_SendTwoMessages_CompletesBoth()
    {
        await using var smtpServer = new LocalSmtpServer();
        using var sender = new SmtpClientEmailSender(CreateSmtpClient(smtpServer));

        var firstTask = sender.SendAsync(CreateMessage("first"));
        var secondTask = sender.SendAsync(CreateMessage("second"));

        await firstTask.WaitAsync(TestTimeout);
        await secondTask.WaitAsync(TestTimeout);

        Assert.Equal(2, smtpServer.Messages.Count);
        Assert.Contains("Subject: first", smtpServer.Messages[0]);
        Assert.Contains("Subject: second", smtpServer.Messages[1]);
    }

    [Fact]
    public async Task SmtpClientEmailSender_SynchronousValidationFailure_FaultsAndContinuesQueue()
    {
        await using var smtpServer = new LocalSmtpServer(delayFirstAcceptedMessage: true);
        using var sender = new SmtpClientEmailSender(CreateSmtpClient(smtpServer));

        var firstTask = sender.SendAsync(CreateMessage("first"));
        await smtpServer.FirstAcceptedMessage.WaitAsync(TestTimeout);

        var invalidTask = sender.SendAsync(new MailMessage());
        var laterTask = sender.SendAsync(CreateMessage("later"));
        smtpServer.ReleaseFirstAcceptedMessage();

        await firstTask.WaitAsync(TestTimeout);
        await Assert.ThrowsAnyAsync<Exception>(() => invalidTask.WaitAsync(TestTimeout));
        await laterTask.WaitAsync(TestTimeout);

        Assert.Equal(2, smtpServer.Messages.Count);
        Assert.Contains("Subject: later", smtpServer.Messages[1]);
    }

    [Fact]
    public async Task SmtpClientEmailSender_AsyncSmtpFailure_FaultsAndContinuesQueue()
    {
        await using var smtpServer = new LocalSmtpServer(
            rejectedRecipient: "reject@example.com",
            delayRejectedResponse: true);
        using var sender = new SmtpClientEmailSender(CreateSmtpClient(smtpServer));

        var rejectedTask = sender.SendAsync(CreateMessage("rejected", "reject@example.com"));
        await smtpServer.RejectedRecipient.WaitAsync(TestTimeout);

        var validTask = sender.SendAsync(CreateMessage("valid"));
        smtpServer.ReleaseRejectedRecipient();

        await Assert.ThrowsAnyAsync<Exception>(() => rejectedTask.WaitAsync(TestTimeout));
        await validTask.WaitAsync(TestTimeout);

        Assert.Single(smtpServer.Messages);
        Assert.Contains("Subject: valid", smtpServer.Messages[0]);
    }

    private static SmtpClient CreateSmtpClient(LocalSmtpServer smtpServer) => new SmtpClient
    {
        Host = "127.0.0.1",
        Port = smtpServer.Port,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        EnableSsl = false,
        Timeout = 5000
    };

    private static MailMessage CreateMessage(string subject, string recipient = "recipient@example.com") =>
        new MailMessage("sender@example.com", recipient)
        {
            Subject = subject,
            Body = subject
        };
}
