// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Emails
{
    using System;

    /// <summary>
    /// Exception occures when email sending queue is overloaded.
    /// </summary>
    public class EmailQueueExceededException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="maxSize">Max queue size that has benn exceeded.</param>
        public EmailQueueExceededException(int maxSize) :
            base($"Maximum email queue size {maxSize} exceeded.")
        {
        }
    }
}
