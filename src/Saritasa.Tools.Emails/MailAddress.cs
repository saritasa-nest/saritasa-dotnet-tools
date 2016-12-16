// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_6
namespace Saritasa.Tools.Emails
{
    using System;

    /// <summary>
    /// Represents the address of an electronic mail sender or recipient. Very raw implementation
    /// for frameworks that do not support MailMessage API.
    /// </summary>
    public class MailAddress
    {
        /// <summary>
        /// Display name and address information.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// User information.
        /// </summary>
        public string User { get; }

        /// <summary>
        /// E-mail address.
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="address">E-mail address.</param>
        /// <param name="user">User information.</param>
        /// <param name="displayName">User display name.</param>
        public MailAddress(string address, string user = null, string displayName = null)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            Address = address;
            User = user;
            DisplayName = !string.IsNullOrEmpty(displayName) ? displayName : address;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return !string.IsNullOrEmpty(DisplayName) ? DisplayName : Address;
        }
    }
}
#endif
