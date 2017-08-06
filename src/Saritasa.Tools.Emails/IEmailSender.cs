// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452
using System.Net.Mail;
#endif
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails
{
    /// <summary>
    /// Email sender interface.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="mailMessage">The mail message.</param>
        /// <returns>Task with email sending operation.</returns>
        Task SendAsync(MailMessage mailMessage);
    }
}
