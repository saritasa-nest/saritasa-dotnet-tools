// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails;

/// <summary>
/// Send email using <see cref="SmtpClient" />. The class is thread-safe and allows
/// concurrent use of SendAsync method.
/// </summary>
public class SmtpClientEmailSender : EmailSender, IDisposable
{
    /// <summary>
    /// We provide another task to client since actual email sending can be delayed
    /// (enqueued).
    /// </summary>
    private readonly struct MailMessageWithTaskSource
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

    /// <summary>
    /// Use Send method instead of SendMailAsync. When <c>true</c> the calling
    /// thread will be blocked.
    /// </summary>
    public bool UseSyncMode { get; set; }

    private DateTime lastSendTime = DateTime.Now;

    /// <summary>
    /// Constructor.
    /// </summary>
    public SmtpClientEmailSender()
    {
        Client = new SmtpClient();
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


    /// <inheritdoc />
    protected override Task Process(MailMessage message, IDictionary<string, object>? data)
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
    internal Task SendAsyncInternal(MailMessage message, IDictionary<string, object>? data) => Process(message, data);

    /// <summary>
    /// Starts the single queue worker when no worker is already active.
    /// </summary>
    private void ProcessInternal()
    {
        lock (@lock)
        {
            if (isBusy)
            {
                return;
            }

            isBusy = true;
        }

        // Claim worker ownership under the lock, but run all SMTP operations outside it.
        _ = ProcessQueueAsync();
    }

    /// <summary>
    /// Pumps queued messages sequentially and completes each caller task from the
    /// terminal state of its SMTP operation.
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        try
        {
            while (true)
            {
                // The worker owns queue progression until it observes an empty queue.
                MailMessageWithTaskSource messageTask;
                TimeSpan delay;

                lock (@lock)
                {
                    // Check this under the same lock used by ProcessInternal so a newly
                    // enqueued message cannot be missed while the worker is shutting down.
                    if (queue.IsEmpty)
                    {
                        return;
                    }

                    // Throttle send starts so MinDelay is measured between message starts.
                    delay = MinDelay - (DateTime.Now - lastSendTime);
                    if (delay <= TimeSpan.Zero)
                    {
                        queue.TryDequeue(out messageTask);
                        lastSendTime = DateTime.Now;
                    }
                    else
                    {
                        messageTask = default;
                        // Keep the message queued while waiting; this worker remains the only pump.
                    }
                }

                // Never hold the sender lock while waiting for the throttle interval.
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, CancellationToken.None).ConfigureAwait(false);
                    continue;
                }

                // SendMailAsync may fail synchronously before returning its task, so the call
                // must remain inside the same exception handling as asynchronous failures.
                try
                {
                    if (UseSyncMode)
                    {
                        Client.Send(messageTask.MailMessage);
                    }
                    else
                    {
                        await Client.SendMailAsync(messageTask.MailMessage).ConfigureAwait(false);
                    }

                    // Complete the caller's task before advancing to the next queued message.
                    messageTask.TaskCompletionSource.TrySetResult(true);
                }
                catch (OperationCanceledException)
                {
                    messageTask.TaskCompletionSource.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    messageTask.TaskCompletionSource.TrySetException(ex);
                }
            }
        }
        finally
        {
            bool restartWorker;
            // A producer may enqueue after the loop's empty-queue check. Recheck under the
            // lock before releasing ownership so that message cannot leave the queue stalled.
            lock (@lock)
            {
                isBusy = false;
                restartWorker = !queue.IsEmpty;
                if (restartWorker)
                {
                    isBusy = true;
                }
            }

            // Restart only when a producer won the race to enqueue during worker shutdown.
            if (restartWorker)
            {
                _ = ProcessQueueAsync();
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
                Client.Dispose();
            }
            disposed = true;
        }
    }

    #endregion
}
