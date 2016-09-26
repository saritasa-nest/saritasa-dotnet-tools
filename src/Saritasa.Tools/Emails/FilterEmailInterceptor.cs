// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Filters users to whom send an email. The abstract class that contains general logic to be implemented.
    /// </summary>
    public abstract class FilterEmailInterceptor<TMessage> : IEmailInterceptor<TMessage> where TMessage : class
    {
        readonly IList<string> approvedAddresses = new List<string>();

        /// <summary>
        /// Gets approved addresses. Emails that do not match to these address patterns will not be sent.
        /// Default is all.
        /// </summary>
        public IEnumerable<string> ApprovedAddresses => approvedAddresses;

        /// <summary>
        /// .ctor
        /// </summary>
        protected FilterEmailInterceptor()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="emails">Approved emails patterns. You can use ? and * symbols.</param>
        protected FilterEmailInterceptor(string emails)
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
        public abstract void Sending(TMessage mailMessage, IDictionary<string, object> data, ref bool cancel);

        /// <inheritdoc />
        public abstract void Sent(TMessage mailMessage, IDictionary<string, object> data);

        #endregion

        /// <summary>
        /// Converts wildcards to regex. Determines what reg exp correspond to string with * and ? chars.
        /// </summary>
        /// <param name="pattern">The wildcards pattern.</param>
        /// <returns>Pattern string.</returns>
        protected static string WildcardToRegex(string pattern)
        {
            return ("^" + Regex.Escape(pattern)).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }
    }
}
