// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Exceptions
{
    using System;

    /// <summary>
    /// Validation exception.
    /// </summary>
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class ValidationException : DomainException
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public ValidationException()
        {
        }

        /// <summary>
        /// .ctor with message.
        /// <param name="message">Exception message.</param>
        /// </summary>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor with message and inner exception.
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        /// </summary>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
