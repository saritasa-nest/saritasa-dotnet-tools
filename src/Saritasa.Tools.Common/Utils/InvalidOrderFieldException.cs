// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Utils;

/// <summary>
/// The exception occurs when the provided by user field has not been found
/// at target list of available ones.
/// </summary>
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
[Serializable]
#endif
public class InvalidOrderFieldException : Exception
{
    /// <summary>
    /// Available fields that can be used for ordering.
    /// </summary>
    public string[] AvailableFields { get; } = new string[0];

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="fieldName">Order field name.</param>
    /// <param name="availableFields">Available fields that can be used for ordering.</param>
    internal InvalidOrderFieldException(string fieldName, string[] availableFields) :
        base(string.Format(Properties.Strings.OrderByFieldIsNotSupported, fieldName, string.Join(", ", availableFields)))
    {
        AvailableFields = availableFields;
    }

#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Constructor for deserialization.
    /// </summary>
    /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
    /// <param name="context">Describes the source and destination of a given serialized stream,
    /// and provides an additional caller-defined context.</param>
    protected InvalidOrderFieldException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
    }
#endif
}
