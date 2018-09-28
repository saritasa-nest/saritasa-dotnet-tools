// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails.Interceptors
{
    /// <summary>
    /// Saves emails into specified folder in .eml format.
    /// </summary>
    public class SaveToFileEmailInterceptor : IEmailInterceptor
    {
        /// <summary>
        /// Directory to save emails.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// Save only sent emails.
        /// </summary>
        public bool AfterSend { get; }

        private static readonly object @lock = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">Directory to save emails.</param>
        /// <param name="afterSend">Save only sent emails.</param>
        public SaveToFileEmailInterceptor(string directory, bool afterSend = false)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }
            Directory = directory;
            AfterSend = afterSend;
        }

        private void Save(MailMessage message)
        {
            // Sometimes we have emails sending at the same second, add postfix in that case.
            lock (@lock)
            {
                var path = Path.Combine(Directory, $"{DateTime.Now:yyyyMMdd-hhmmss}.msg");
                for (int i = 0; File.Exists(path); i++)
                {
                    path = Path.Combine(Directory, $"{DateTime.Now:yyyyMMdd-hhmmss}-{i:000}.msg");
                }
                SaveMailMessage(message, path);
            }
        }

        #region IEmailInterceptor implementation

        /// <inheritdoc />
        public Task SendingAsync(MailMessage mailMessage, IDictionary<string, object> data, ref bool cancel,
            CancellationToken cancellationToken)
        {
            if (!AfterSend)
            {
                Save(mailMessage);
            }
            return Internals.TaskHelpers.CompletedTask;
        }

        /// <inheritdoc />
        public Task SentAsync(MailMessage mailMessage, IDictionary<string, object> data,
            CancellationToken cancellationToken)
        {
            if (AfterSend)
            {
                Save(mailMessage);
            }
            return Internals.TaskHelpers.CompletedTask;
        }

        #endregion

        /// <summary>
        /// Saves the specified message to disk.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fileName">Name of the file.</param>
        private static void SaveMailMessage(MailMessage message, string fileName)
        {
            var assembly = typeof(SmtpClient).Assembly;
            var mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                // Get reflection info for MailWriter constructor.
                var mailWriterConstructor =
                    mailWriterType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(Stream) },
                        null);

                // Construct MailWriter object with our FileStream.
                var mailWriter = mailWriterConstructor.Invoke(new object[] { fileStream });

                // Get reflection info for Send() method on MailMessage.
                var sendMethod =
                    typeof(MailMessage).GetMethod(
                        "Send",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call method passing in MailWriter.
                if (sendMethod.GetParameters().Length == 2)
                {
                    sendMethod.Invoke(
                        message,
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] { mailWriter, true },
                        null);
                }
                else
                {
                    sendMethod.Invoke(
                        message,
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] { mailWriter, true, true },
                        null);
                }

                // Finally get reflection info for Close() method on our MailWriter.
                var closeMethod =
                    mailWriter.GetType().GetMethod(
                        "Close",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method.
                closeMethod.Invoke(
                    mailWriter,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { },
                    null);
            }
        }
    }
}
