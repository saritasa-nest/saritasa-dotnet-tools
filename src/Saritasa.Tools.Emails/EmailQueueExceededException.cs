// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Saritasa.Tools.Emails;

/// <summary>
/// Exception occurs when email sending queue is overloaded.
/// </summary>
[Serializable]
public class EmailQueueExceededException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="maxSize">Max queue size that has been exceeded.</param>
    public EmailQueueExceededException(int maxSize) : base(
        string.Format(Properties.Strings.EmailQueueSizeExceed, maxSize.ToString()))
    {
    }

    /// <summary>
    /// Constructor for deserialization.
    /// </summary>
    /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
    /// <param name="context">Describes the source and destination of a given serialized stream,
    /// and provides an additional caller-defined context.</param>
    protected EmailQueueExceededException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
