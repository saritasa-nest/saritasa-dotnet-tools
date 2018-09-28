// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails
{
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
        private struct MailMessageWithTaskSource
        {
            /// <summary>
            /// Completion source for task.
            /// </summary>
            public TaskCompletionSource<bool> TaskCompletionSource { get; }

            /// <summary>
            /// Mail message.
            /// </summary>
            public MailMessage MailMessage { get; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="mailMessage">Mail message.</param>
            public MailMessageWithTaskSource(MailMessage mailMessage)
            {
                MailMessage = mailMessage;
                TaskCompletionSource = new TaskCompletionSource<bool>();
            }
        }

        /// <summary>
        /// Pending email messages queue.
        /// </summary>
        private readonly ConcurrentQueue<MailMessageWithTaskSource> queue =
            new ConcurrentQueue<MailMessageWithTaskSource>();

        /// <summary>
        /// Indicates the email message is currently sending.
        /// </summary>
        private bool isBusy;

        private readonly object @lock = new object();

        /// <summary>
        /// Instance of SmtpClient.
        /// </summary>
        public SmtpClient Client { get; private set; }

        /// <summary>
        /// Maximum queue size. If queue size for some reason is exceeded the
        /// <see cref="EmailQueueExceededException" /> exception will be thrown.
        /// </summary>
        public int MaxQueueSize { get; set; } = 10240;

        /// <summary>
        /// Minimum delay between emails sending. Zero by default.
        /// </summary>
        public TimeSpan MinDelay { get; } = TimeSpan.Zero;

        private DateTime lastSendTime = DateTime.Now;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SmtpClientEmailSender()
        {
            Client = new SmtpClient();
            Client.SendCompleted += OnEmailSent;
        }

        /// <summary>
        /// Constructor.
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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="smtpClient">Smtp client.</param>
        /// <param name="minDelay">Minimum delay between emails sending.</param>
        public SmtpClientEmailSender(SmtpClient smtpClient, TimeSpan minDelay) : this(smtpClient)
        {
            MinDelay = minDelay;
            lastSendTime -= minDelay;
        }

        private void OnEmailSent(object sender, AsyncCompletedEventArgs args)
        {
            // The callback is called before the task.
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
        internal Task SendAsyncInternal(MailMessage message, IDictionary<string, object> data) => Process(message, data);

        private bool isDelayScheduled;

        private void ProcessInternal()
        {
            /*
             * Lock should be used since there possible race condition when we check isBusy field
             * and set if to true. If another thread sends email and isBusy is true no need to
             * actually call Client.SendMailAsync() since it means that OnEmailSent will be called later and
             * ProcessInternal will be called anyway. Because actual email sending can be delayed we return to user
             * our own Task and sync its status with on is returned by Client.SendMailAsync() call.
             * */
            lock (@lock)
            {
                if (isBusy || ScheduleDelay())
                {
                    return;
                }

                if (queue.TryDequeue(out var messageTask))
                {
                    isBusy = true;
                    try
                    {
                        lastSendTime = DateTime.Now;
                        Client.SendMailAsync(messageTask.MailMessage).ContinueWith(t =>
                        {
                            // Sync current task status (from email) with one that is waited by user.
                            if (t.IsFaulted)
                            {
                                if (t.Exception?.InnerExceptions != null)
                                {
                                    messageTask.TaskCompletionSource.SetException(t.Exception.InnerExceptions);
                                }
                                else
                                {
                                    messageTask.TaskCompletionSource.SetException(
                                        new InvalidOperationException("Unexpected exception."));
                                }
                            }
                            else if (t.IsCanceled)
                            {
                                messageTask.TaskCompletionSource.SetCanceled();
                            }
                            else if (t.IsCompleted)
                            {
                                messageTask.TaskCompletionSource.SetResult(true);
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

        /// <summary>
        /// Schedule delay between emails send.
        /// </summary>
        /// <returns>Delay is scheduled and no need to do email send.</returns>
        private bool ScheduleDelay()
        {
            if (MinDelay == TimeSpan.Zero)
            {
                return false;
            }
            var diff = DateTime.Now - lastSendTime;
            if (diff <= MinDelay)
            {
                if (isDelayScheduled)
                {
                    return true;
                }
                isDelayScheduled = true;
                Task.Delay(MinDelay - diff, CancellationToken)
                    .ContinueWith(t =>
                    {
                        isDelayScheduled = false;
                        OnEmailSent(this, null);
                    }, CancellationToken)
                    .ConfigureAwait(false);
                return true;
            }
            return false;
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
