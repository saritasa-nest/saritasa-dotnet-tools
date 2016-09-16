// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Email sender interface.
    /// </summary>
    public interface IEmailSender<TMessage> where TMessage : class
    {
        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        Task SendAsync(TMessage message);
    }
}
