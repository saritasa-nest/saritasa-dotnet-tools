// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    using System.Net.Mail;
#endif
    using System.Threading.Tasks;
    using System.Threading;

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
        /// Execution strategy. DefaultEmailExecutionStrategy used by default. Determines the way how we should proceed
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

            // run pre process interceptors
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

            // send email and run post process interceptors
            return ExecutionStrategy.Execute(Process, message, data, CancellationToken).ContinueWith((task) =>
            {
                if (task.IsFaulted == false)
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
