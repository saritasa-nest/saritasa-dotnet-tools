// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails;

/// <summary>
/// Email interceptor to be used with <see cref="EmailSender" />.
/// </summary>
public interface IEmailInterceptor
{
    /// <summary>
    /// The method is called after success email sending.
    /// </summary>
    /// <param name="mailMessage">Mail message.</param>
    /// <param name="data">Additional data.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task SentAsync(MailMessage mailMessage, IDictionary<string, object> data,
        CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>
    /// The method is called before email sending.
    /// </summary>
    /// <param name="mailMessage">Mail message.</param>
    /// <param name="data">Additional data.</param>
    /// <param name="cancel">Should the email sending be cancelled.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task SendingAsync(
        MailMessage mailMessage,
        IDictionary<string, object> data,
        ref bool cancel,
        CancellationToken cancellationToken = default(CancellationToken));
}
