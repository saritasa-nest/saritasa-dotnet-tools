// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Threading;

namespace Saritasa.Tools.Emails
{
    using NameValueDict = System.Collections.Generic.IDictionary<string, object>;

    /// <summary>
    /// Abstract email sender implementation with interceptors support.
    /// </summary>
    public abstract class EmailSender : IEmailSender
    {
        readonly IList<IEmailInterceptor> interceptors = new List<IEmailInterceptor>();

        /// <summary>
        /// Send message.
        /// </summary>
        protected abstract Task Process(MailMessage message, NameValueDict data);

        /// <summary>
        /// Execution strategy. <see cref="DefaultEmailExecutionStrategy" /> used by default. Determines the way how we should proceed
        /// actual email sending.
        /// </summary>
        protected IEmailExecutionStrategy ExecutionStrategy { get; } = new DefaultEmailExecutionStrategy();

        /// <summary>
        /// Cancellation token instance. None by default.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <inheritdoc />
        public Task SendAsync(MailMessage message)
        {
            var data = new Dictionary<string, object>();
            bool cancel = false;

            // Run pre process interceptors.
            foreach (var interceptor in interceptors)
            {
                interceptor.Sending(message, data, ref cancel);
                if (cancel)
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

            // Send email and run post process interceptors.
            return ExecutionStrategy.Execute(Process, message, data, CancellationToken).ContinueWith(task =>
            {
                if (!task.IsFaulted)
                {
                    foreach (var interceptor in interceptors)
                    {
                        interceptor.Sent(message, data);
                    }
                }
            }, CancellationToken);
        }

        /// <summary>
        /// Add interceptor.
        /// </summary>
        /// <param name="interceptor">Interceptor.</param>
        public EmailSender AddInterceptor(IEmailInterceptor interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }
            interceptors.Add(interceptor);
            return this;
        }
    }
}
