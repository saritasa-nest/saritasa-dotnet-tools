//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using Interfaces;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Saves emails into specified folder in .eml format.
    /// </summary>
    public class SaveToFileEmailInterceptor : IEmailInterceptor
    {
        /// <summary>
        /// Directory to save emails.
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// Save only sent emails.
        /// </summary>
        public bool AfterSend { get; private set; }

        /// <summary>
        /// .ctor
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
            var path = Path.Combine(Directory, string.Format("{0}.msg", DateTime.Now.ToString("yyyyMMdd-hhmmss")));
            SaveMailMessage(message, path);
        }

        #region IEmailInterceptor

        /// <inheritdoc />
        public void Sending(MailMessage mailMessage, IDictionary<string, object> data, ref bool cancel)
        {
            if (AfterSend == false)
            {
                Save(mailMessage);
            }
        }

        /// <inheritdoc />
        public void Sent(MailMessage mailMessage, IDictionary<string, object> data)
        {
            if (AfterSend)
            {
                Save(mailMessage);
            }
        }

        #endregion

        /// <summary>
        /// Saves the specified message to disk.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fileName">Name of the file.</param>
        private static void SaveMailMessage(MailMessage message, string fileName)
        {
            Assembly assembly = typeof(SmtpClient).Assembly;
            Type mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                // get reflection info for MailWriter contructor
                ConstructorInfo mailWriterContructor =
                    mailWriterType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(Stream) }, 
                        null);

                // construct MailWriter object with our FileStream
                object mailWriter = mailWriterContructor.Invoke(new object[] { fileStream });

                // get reflection info for Send() method on MailMessage
                MethodInfo sendMethod =
                    typeof(MailMessage).GetMethod(
                        "Send",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // call method passing in MailWriter
                if (sendMethod.GetParameters().Length == 2)
                {
                    sendMethod.Invoke(
                        message,
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new object[] { mailWriter, true },
                        null);
                }
                else
                {
                    sendMethod.Invoke(
                        message,
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new object[] { mailWriter, true, true },
                        null);
                }

                // finally get reflection info for Close() method on our MailWriter
                MethodInfo closeMethod =
                    mailWriter.GetType().GetMethod(
                        "Close",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // call close method
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
