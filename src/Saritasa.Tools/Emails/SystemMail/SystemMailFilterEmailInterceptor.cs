// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
namespace Saritasa.Tools.Emails.SystemMail
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Filters users to whom send an email.
    /// </summary>
    public class SystemMailFilterEmailInterceptor : FilterEmailInterceptor<MailMessage>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public SystemMailFilterEmailInterceptor()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="emails">Approved emails patterns. You can use ? and * symbols.</param>
        public SystemMailFilterEmailInterceptor(string emails)
        {
            SetApprovedEmails(emails);
        }

        #region IEmailInterceptor

        /// <inheritdoc />
        public override void Sending(MailMessage mailMessage, IDictionary<string, object> data, ref bool cancel)
        {
            FilterAddress(mailMessage.To);
            FilterAddress(mailMessage.CC);
            FilterAddress(mailMessage.Bcc);

            if (mailMessage.To.Count == 0 && mailMessage.CC.Count == 0 && mailMessage.Bcc.Count == 0)
            {
                cancel = true;
            }
        }

        /// <inheritdoc />
        public override void Sent(MailMessage mailMessage, IDictionary<string, object> data)
        {
        }

        #endregion

        /// <summary>
        /// Filters the collection of addresses by approved addresses.
        /// </summary>
        private void FilterAddress(MailAddressCollection addressCollection)
        {
            if (addressCollection.Count < 1)
            {
                return;
            }

            var badAddresses = new MailAddressCollection();

            foreach (var address in addressCollection)
            {
                bool match = false;
                foreach (var pattern in ApprovedAddresses)
                {
                    if (Regex.IsMatch(address.Address, WildcardToRegex(pattern)))
                    {
                        match = true;
                    }
                }

                if (!match)
                {
                    badAddresses.Add(address);
                }
            }

            foreach (var badAddress in badAddresses)
            {
                addressCollection.Remove(badAddress);
            }
        }
    }
}
#endif
