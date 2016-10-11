// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Emails;

    /// <summary>
    /// Empty email sender. Can be used for testing.
    /// </summary>
    public class DummyEmailSender<TMessage> : EmailSender<TMessage> where TMessage : class
    {
        /// <inheritdoc />
        protected override Task Process(TMessage message, IDictionary<string, object> data)
        {
#if NET452
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            return tcs.Task;
#else
            return Task.CompletedTask;
#endif
        }
    }
}
