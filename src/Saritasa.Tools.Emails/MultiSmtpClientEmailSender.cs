// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NET452
namespace Saritasa.Tools.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles several smtp clients to send emails using round-robin method.
    /// </summary>
    public class MultiSmtpClientEmailSender : EmailSender, IDisposable
    {
        SmtpClientEmailSender[] clientInstances;

        readonly object @lock = new object();

        int currentInstanceIndex;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="smtpClientInstancesCount">Max number of instances. 2 by default.</param>
        /// <param name="smtpClient">Smtp client to be used as reference object. If null default settings
        /// from configuration will be used.</param>
        public MultiSmtpClientEmailSender(int smtpClientInstancesCount = 2, SmtpClient smtpClient = null)
        {
            if (smtpClientInstancesCount <= 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(Properties.Strings.ArgumentMustBeGreaterThan,
                    smtpClientInstancesCount, "zero"));
            }
            clientInstances = new SmtpClientEmailSender[smtpClientInstancesCount];
            for (int i = 0; i < smtpClientInstancesCount; i++)
            {
                var clonedSmtpClient = CloneSmtpClient(smtpClient);
                clientInstances[i] = new SmtpClientEmailSender(clonedSmtpClient);
                clientInstances[i].Client.ServicePoint.ConnectionLimit = smtpClientInstancesCount;
            }
        }

        /// <summary>
        /// Clones <see cref="SmtpClient" /> instance to new one. If null just creates it.
        /// </summary>
        /// <param name="client">Original smtp client instance.</param>
        /// <returns>Cloned smtp client instance.</returns>
        private static SmtpClient CloneSmtpClient(SmtpClient client)
        {
            if (client == null)
            {
                return new SmtpClient();
            }

            var newClient = new SmtpClient(client.Host, client.Port)
            {
                Credentials = client.Credentials,
                UseDefaultCredentials = client.UseDefaultCredentials,
                DeliveryFormat = client.DeliveryFormat,
                DeliveryMethod = client.DeliveryMethod,
                EnableSsl = client.EnableSsl,
                PickupDirectoryLocation = client.PickupDirectoryLocation,
                Timeout = client.Timeout,
            };
            return newClient;
        }

        private SmtpClientEmailSender GetNextClient()
        {
            if (currentInstanceIndex + 1 >= clientInstances.Length)
            {
                currentInstanceIndex = 0;
            }
            else
            {
                currentInstanceIndex++;
            }

            return clientInstances[currentInstanceIndex];
        }

        /// <inheritdoc />
        protected override Task Process(MailMessage message, IDictionary<string, object> data)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            lock (@lock)
            {
                return GetNextClient().SendAsyncInternal(message, data);
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
                    foreach (var client in clientInstances)
                    {
                        client.Dispose();
                    }
                    clientInstances = null;
                }
                disposed = true;
            }
        }

#endregion
    }
}
#endif
