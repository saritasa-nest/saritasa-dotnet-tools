// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Net.Mail;

namespace Saritasa.Tools.Emails;

/// <summary>
/// Extensions to <see cref="IEmailSender" />.
/// </summary>
public static class EmailSenderExtensions
{
    /// <summary>
    /// Sync implementation of SendAsync method. Sends messages and configures await to false.
    /// </summary>
    /// <param name="emailSender">Email sender instance.</param>
    /// <param name="message">Message to send.</param>
    public static void Send(this IEmailSender emailSender, MailMessage message)
    {
        emailSender.SendAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
