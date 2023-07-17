// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Saritasa.Tools.Domain.Exceptions;

/// <summary>
/// Domain forbidden security exception. Can be mapped to the 403 HTTP status code.
/// </summary>
[Serializable]
public class ForbiddenException : DomainException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public ForbiddenException() : base(DomainErrorDescriber.Default.Forbidden())
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="code">Optional description code for this exception.</param>
    public ForbiddenException(int code) : base(DomainErrorDescriber.Default.Forbidden(), code)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ForbiddenException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="code">Optional description code for this exception.</param>
    public ForbiddenException(string message, int code) : base(message, code)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="code">Optional description code for this exception.</param>
    public ForbiddenException(string message, string code) : base(message, code)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    /// <param name="code">Optional description code for this exception.</param>
    public ForbiddenException(string message, Exception innerException, int code) :
        base(message, innerException, code)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    /// <param name="code">Optional description code for this exception.</param>
    public ForbiddenException(string message, Exception innerException, string code) :
        base(message, innerException, code)
    {
    }

    /// <summary>
    /// Constructor for deserialization.
    /// </summary>
    /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
    /// <param name="context">Describes the source and destination of a given serialized stream,
    /// and provides an additional caller-defined context.</param>
    protected ForbiddenException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
