// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions.Queries
{
    /// <summary>
    /// Message context extensions.
    /// </summary>
    public static class MessageContextExtensions
    {
        /// <summary>
        /// Gets result value from items.
        /// </summary>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="messageContext">Message context.</param>
        /// <returns>Result.</returns>
        public static TResult GetResult<TResult>(this IMessageContext messageContext)
        {
            return (TResult)messageContext.Items[MessageContextConstants.ResultKey];
        }
    }
}
