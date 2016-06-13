// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;

    /// <summary>
    /// Email interceptor to be used with EmailSender.
    /// </summary>
    public interface IEmailInterceptor
    {
        /// <summary>
        /// The method is called after email sending.
        /// </summary>
        /// <param name="mailMessage">Mail message.</param>
        /// <param name="data">Additional data.</param>
        void Sent(MailMessage mailMessage, IDictionary<string, object> data);

        /// <summary>
        /// The mail is called before email sending.
        /// </summary>
        /// <param name="mailMessage">Mail message.</param>
        /// <param name="data">Additional data.</param>
        /// <param name="cancel">Should the email sending be cancelled.</param>
        void Sending(MailMessage mailMessage, IDictionary<string, object> data, ref bool cancel);
    }
}
