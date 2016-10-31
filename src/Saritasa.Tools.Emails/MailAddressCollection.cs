// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NETCOREAPP1_0 || NETSTANDARD1_6
namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Store e-mail addresses that are associated with an e-mail message. Very raw implementation for
    /// framework that do not support MailMessage API.
    /// </summary>
    public class MailAddressCollection : Collection<MailAddress>
    {
    }
}
#endif
