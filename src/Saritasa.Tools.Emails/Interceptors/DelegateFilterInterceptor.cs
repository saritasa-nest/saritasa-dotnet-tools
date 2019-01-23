// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails.Interceptors
{
    /// <summary>
    /// The interceptor specifies delegate to filter email addresses.
    /// </summary>
    public class DelegateFilterInterceptor : IEmailInterceptor
    {
        private readonly Func<string, bool> filterFunc = email => false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DelegateFilterInterceptor()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filterFunc">Filter delegate the accepts email as argument and returns bool.</param>
        public DelegateFilterInterceptor(Func<string, bool> filterFunc) : this()
        {
            if (filterFunc == null)
            {
                throw new ArgumentNullException(nameof(filterFunc));
            }
            this.filterFunc = filterFunc;
        }

        #region IEmailInterceptor implementation

        /// <inheritdoc />
        public virtual Task SendingAsync(MailMessage mailMessage, IDictionary<string, object> data, ref bool cancel,
            CancellationToken cancellationToken)
        {
            FilterAddress(mailMessage.To);
            FilterAddress(mailMessage.CC);
            FilterAddress(mailMessage.Bcc);

            if (mailMessage.To.Count == 0 && mailMessage.CC.Count == 0 && mailMessage.Bcc.Count == 0)
            {
                cancel = true;
            }
            return Internals.TaskHelpers.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task SentAsync(MailMessage mailMessage, IDictionary<string, object> data,
            CancellationToken cancellationToken) => Internals.TaskHelpers.CompletedTask;

        #endregion

        /// <summary>
        /// The method returns <c>true</c> if email should not be send.
        /// Otherwise returns <c>false</c>.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <returns><c>True</c> to filter, otherwise <c>false</c>.</returns>
        protected virtual bool ShouldEmailBeFiltered(string email) => filterFunc(email);

        /// <summary>
        /// Filters the collection of addresses.
        /// </summary>
        private void FilterAddress(MailAddressCollection addressCollection)
        {
            if (addressCollection.Count < 1)
            {
                return;
            }

            var addressesToFilter = new MailAddressCollection();

            foreach (var address in addressCollection)
            {
                if (ShouldEmailBeFiltered(address.Address))
                {
                    addressesToFilter.Add(address);
                }
            }

            foreach (var badAddress in addressesToFilter)
            {
                addressCollection.Remove(badAddress);
            }
        }
    }
}
