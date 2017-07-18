// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NETSTANDARD1_3
using System;

namespace Saritasa.Tools.Emails
{
    /// <summary>
    /// Very raw and basic implementation of MailMessage for frameworks that do not support MailMessage API.
    /// Itended to be replaced.
    /// </summary>
    public class MailMessage : IDisposable
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public MailMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MailMessage class by using the specified System.String class objects.
        /// </summary>
        /// <param name="from">From email address, user who sent the message.</param>
        /// <param name="to">To email address.</param>
        public MailMessage(string from, string to)
        {
            From = new MailAddress(from);
            To.Add(new MailAddress(to));
        }

        /// <summary>
        /// Gets or sets the from address for this e-mail message.
        /// </summary>
        public MailAddress From { get; set; }

        /// <summary>
        /// Recipients of the e-mail message.
        /// </summary>
        public MailAddressCollection To { get; } = new MailAddressCollection();

        /// <summary>
        /// Carbon copy (CC) recipients for the e-mail message.
        /// </summary>
        public MailAddressCollection CC { get; } = new MailAddressCollection();

        /// <summary>
        /// Blind carbon copy (BCC) recipients for the e-mail message.
        /// </summary>
        public MailAddressCollection Bcc { get; } = new MailAddressCollection();

        /// <summary>
        /// Subject line of email message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Message body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Is message body in html format.
        /// </summary>
        public bool IsBodyHtml { get; set; }

        private bool disposed;

        /// <summary>
        /// Releases all resources used by the MailMessage
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MailMessage and
        /// optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// False to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.disposed = true;
            }
        }
    }
}
#endif
