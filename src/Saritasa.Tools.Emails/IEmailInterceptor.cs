// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
#if NET452
    using System.Net.Mail;
#endif
    using JetBrains.Annotations;

    /// <summary>
    /// Email interceptor to be used with EmailSender.
    /// </summary>
    public interface IEmailInterceptor
    {
        /// <summary>
        /// The method is called after success email sending.
        /// </summary>
        /// <param name="mailMessage">Mail message.</param>
        /// <param name="data">Additional data.</param>
        void Sent([NotNull] MailMessage mailMessage, [NotNull] IDictionary<string, object> data);

        /// <summary>
        /// The method is called before email sending.
        /// </summary>
        /// <param name="mailMessage">Mail message.</param>
        /// <param name="data">Additional data.</param>
        /// <param name="cancel">Should the email sending be cancelled.</param>
        void Sending(
            [NotNull] MailMessage mailMessage,
            [NotNull] IDictionary<string, object> data,
            ref bool cancel);
    }
}
