// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using NameValueDict = System.Collections.Generic.IDictionary<string, object>;

    /// <summary>
    /// This execution strategy does not do any operation.
    /// </summary>
    public class DefaultEmailExecutionStrategy<TMessage> : IEmailExecutionStrategy<TMessage> where TMessage : class
    {
        /// <inheritdoc />
        public Task Execute(Func<TMessage, NameValueDict, Task> handler, TMessage message, NameValueDict data, CancellationToken cancellationToken)
        {
            return handler(message, data);
        }
    }
}
