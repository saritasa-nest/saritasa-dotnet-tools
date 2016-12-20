// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.


#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net.Mail;
    using System.Threading.Tasks;

    /// <summary>
    /// Send email using SmtpClient. The class is thread safe and allows multiple calls
    /// of SendAsync method.
    /// </summary>
    public class SmtpClientEmailSender : EmailSender, IDisposable
    {
        /// <summary>
        /// We provide another task to client since actual email sending can be delayed
        /// (enqueued).
        /// </summary>
        struct MailMessageWithTaskSource
        {
            /// <summary>
            /// Completion source for task.
            /// </summary>
            public TaskCompletionSource<int> TaskCompletionSource { get; }

            /// <summary>
            /// Mail message.
            /// </summary>
            public MailMessage MailMessage { get; }

            /// <summary>
            /// .ctor
            /// </summary>
            /// <param name="mailMessage">Mail message.</param>
            public MailMessageWithTaskSource(MailMessage mailMessage)
            {
                MailMessage = mailMessage;
                TaskCompletionSource = new TaskCompletionSource<int>();
            }
        }

        readonly ConcurrentQueue<MailMessageWithTaskSource> queue =
            new ConcurrentQueue<MailMessageWithTaskSource>();

        /// <summary>
        /// Indicates the email message is currently sending.
        /// </summary>
        bool isBusy;

        readonly object @lock = new object();

        /// <summary>
        /// Instance of SmtpClient.
        /// </summary>
        public SmtpClient Client { get; private set; }

        /// <summary>
        /// Maximum queue size. If queue size for some reason is exceeded the
        /// <see cref="EmailQueueExceededException" /> exception will be thrown.
        /// </summary>
        public int MaxQueueSize = 10240;

        /// <summary>
        /// .ctor
        /// </summary>
        public SmtpClientEmailSender()
        {
            Client = new SmtpClient();
            Client.SendCompleted += OnEmailSent;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="smtpClient">Smtp client.</param>
        public SmtpClientEmailSender(SmtpClient smtpClient)
        {
            if (smtpClient == null)
            {
                throw new ArgumentNullException(nameof(smtpClient));
            }

            Client = smtpClient;
            Client.SendCompleted += OnEmailSent;
        }

        private void OnEmailSent(object sender, AsyncCompletedEventArgs args)
        {
            // the callback is called before the task
            isBusy = false;
            ProcessInternal();
        }

        /// <inheritdoc />
        protected override Task Process(MailMessage message, IDictionary<string, object> data)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            var messageTask = new MailMessageWithTaskSource(message);
            queue.Enqueue(messageTask);
            if (queue.Count > MaxQueueSize)
            {
                throw new EmailQueueExceededException(MaxQueueSize);
            }
            ProcessInternal();
            return messageTask.TaskCompletionSource.Task;
        }

        /// <summary>
        /// Calls email processing method directly skipping all interceptors.
        /// </summary>
        /// <param name="message">Mail message.</param>
        /// <param name="data">Additional data.</param>
        /// <returns>Async task operation.</returns>
        internal Task SendAsyncInternal(MailMessage message, IDictionary<string, object> data)
        {
            return Process(message, data);
        }

        private void ProcessInternal()
        {
            lock (@lock)
            {
                if (isBusy)
                {
                    return;
                }

                MailMessageWithTaskSource messageTask;
                if (queue.TryDequeue(out messageTask))
                {
                    isBusy = true;
                    try
                    {
                        Client.SendMailAsync(messageTask.MailMessage).ContinueWith(t =>
                        {
                            isBusy = false;

                            // sync current task status (from email) with one that is waited by user
                            if (t.IsFaulted && t.Exception?.InnerExceptions != null)
                            {
                                // TODO
                                messageTask.TaskCompletionSource.SetException(t.Exception.InnerExceptions);
                            }
                            else if (t.IsCanceled)
                            {
                                messageTask.TaskCompletionSource.SetCanceled();
                            }
                            else if (t.IsCompleted)
                            {
                                messageTask.TaskCompletionSource.SetResult(0);
                            }
                        });
                    }
                    catch (Exception)
                    {
                        isBusy = false;
                    }
                }
            }
        }

        #region Dispose

        private bool disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object.
        /// </summary>
        /// <param name="disposing">Dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Client.SendCompleted -= OnEmailSent;
                    Client.Dispose();
                    Client = null;
                }
                disposed = true;
            }
        }

        #endregion
    }
}
#endif
