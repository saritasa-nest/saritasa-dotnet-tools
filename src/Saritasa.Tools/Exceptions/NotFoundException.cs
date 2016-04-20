//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Exceptions
{
    using System;

    /// <summary>
    /// Exception occurs in domain part of application if entity is not found by key.
    /// </summary>
    [Serializable]
    public class NotFoundException : DomainException
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public NotFoundException() : base()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public NotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
