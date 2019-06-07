// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.EF.ObjectContext
{
    /// <summary>
    /// Invalid entity key exception.
    /// </summary>
    [Serializable]
    public class InvalidKeyException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public InvalidKeyException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public InvalidKeyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public InvalidKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
