//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Text.RegularExpressions;
    using Interfaces;

    /// <summary>
    /// Filters users to whom send an email.
    /// </summary>
    public class FilterEmailInterceptor : IEmailInterceptor
    {
        private IList<string> approvedAddresses = new List<string>();

        /// <summary>
        /// Gets approved addresses. Emails that do not match to these address patterns will not be sent.
        /// Default is all.
        /// </summary>
        public IEnumerable<string> ApprovedAddresses
        {
            get { return approvedAddresses; }
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public FilterEmailInterceptor()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="emails">Approved emails patterns. You can use ? and * symbols.</param>
        public FilterEmailInterceptor(string emails)
        {
            SetApprovedEmails(emails);
        }

        /// <summary>
        /// Adds approved emails. You can use ? and * symbols. If several patterns are specified use comma to separate them.
        /// </summary>
        public void AddApprovedEmails(string emails)
        {
            if (string.IsNullOrEmpty(emails))
            {
                throw new ArgumentNullException(nameof(emails));
            }

            var parsedEmails = emails.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var email in parsedEmails)
            {
                var item = email.ToLowerInvariant().Trim();
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                if (!approvedAddresses.Contains(item))
                {
                    approvedAddresses.Add(item);
                }
            }
        }

        /// <summary>
        /// Sets the approved emails. You can use ? and * symbols. If several patterns are specified use comma to separate them.
        /// </summary>
        /// <param name="emails">The emails.</param>
        public void SetApprovedEmails(string emails)
        {
            if (string.IsNullOrEmpty(emails))
            {
                throw new ArgumentException(nameof(emails));
            }

            approvedAddresses.Clear();
            AddApprovedEmails(emails);
        }

        #region IEmailInterceptor

        /// <inheritdoc />
        public void Sending(MailMessage mailMessage, IDictionary<string, object> data, ref bool cancel)
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
        public void Sent(MailMessage mailMessage, IDictionary<string, object> data)
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
                foreach (var pattern in approvedAddresses)
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

        /// <summary>
        /// Converts wildcards to regex. Determines what reg exp correspond to string with * and ? chars.
        /// </summary>
        /// <param name="pattern">The wildcards pattern.</param>
        /// <returns>Pattern string.</returns>
        private static string WildcardToRegex(string pattern)
        {
            return ("^" + Regex.Escape(pattern)).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }
    }
}
