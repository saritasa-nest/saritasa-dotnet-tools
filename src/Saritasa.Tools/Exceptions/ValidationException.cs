// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Exceptions
{
    using System;

    /// <summary>
    /// Validation exception.
    /// </summary>
    [Serializable]
    public class ValidationException : DomainException
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public ValidationException() : base()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
