// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Threading;

using NameValueDict = System.Collections.Generic.IDictionary<string, object>;

namespace Saritasa.Tools.Emails
{
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

        /// <inheritdoc />
        public async Task SendAsync(MailMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = new Dictionary<string, object>();
            bool cancel = false;

            // Run pre-process interceptors.
            foreach (var interceptor in interceptors)
            {
                await interceptor.SendingAsync(message, data, ref cancel, cancellationToken).ConfigureAwait(false);
                if (cancel)
                {
                    return;
                }
            }

            // Send email and run post process interceptors.
            await ExecutionStrategy.ExecuteAsync(Process, message, data, cancellationToken).ConfigureAwait(false);

            foreach (var interceptor in interceptors)
            {
                await interceptor.SentAsync(message, data, cancellationToken).ConfigureAwait(false);
            }
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
