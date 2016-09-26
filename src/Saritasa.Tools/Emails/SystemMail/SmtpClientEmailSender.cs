// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
namespace Saritasa.Tools.Emails.SystemMail
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;

    /// <summary>
    /// Send email using SmtpClient.
    /// </summary>
    public class SmtpClientEmailSender : EmailSender<MailMessage>
    {
        /// <summary>
        /// Instance of SmtpClient.
        /// </summary>
        public SmtpClient Client { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        public SmtpClientEmailSender()
        {
            Client = new SmtpClient();
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="smtpClient">Smtp client.</param>
        public SmtpClientEmailSender(SmtpClient smtpClient)
        {
            if (smtpClient == null)
            {
                throw new ArgumentNullException(nameof(smtpClient));
            }

            Client = new SmtpClient();
        }

        /// <inheritdoc />
        protected override Task Process(MailMessage message, IDictionary<string, object> data)
        {
            return Client.SendMailAsync(message);
        }
    }
}
#endif
