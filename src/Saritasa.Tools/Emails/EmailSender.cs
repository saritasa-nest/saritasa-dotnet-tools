// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;

    using NameValueDict = System.Collections.Generic.IDictionary<string, object>;

    /// <summary>
    /// Abstract email sender implementation with interceptor support.
    /// </summary>
    public abstract class EmailSender<TMessage> : IEmailSender<TMessage> where TMessage : class
    {
        private IList<IEmailInterceptor<TMessage>> interceptors = new List<IEmailInterceptor<TMessage>>();

        /// <summary>
        /// Send message.
        /// </summary>
        protected abstract Task Process(TMessage message, NameValueDict data);

        /// <summary>
        /// Execution strategy. DefaultEmailExecutionStrategy by default.
        /// </summary>
        protected IEmailExecutionStrategy<TMessage> ExecutionStrategy { get; private set; } = new DefaultEmailExecutionStrategy<TMessage>();

        /// <summary>
        /// Cancellation token instance. None by default.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// .ctor
        /// </summary>
        public EmailSender()
        {
        }

        /// <inheritdoc />
        public Task SendAsync(TMessage message)
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
            });
        }

        /// <summary>
        /// Add interceptor.
        /// </summary>
        /// <param name="interceptor">Interceptor.</param>
        public void AddInterceptor(IEmailInterceptor<TMessage> interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }

            interceptors.Add(interceptor);
        }
    }
}
