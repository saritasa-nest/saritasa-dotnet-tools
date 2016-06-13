// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Dummy
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Emails;

    /// <summary>
    /// Empty email sender. Can be used for testing.
    /// </summary>
    public class DummyEmailSender : EmailSender
    {
        /// <inheritdoc />
        protected override Task Process(MailMessage message, IDictionary<string, object> data)
        {
#if NET4_5
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            return tcs.Task;
#else
            return Task.CompletedTask;
#endif
        }
    }
}
