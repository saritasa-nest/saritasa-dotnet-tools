// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// The exception occurs when the provided by user field has not been found
    /// at target list of available ones.
    /// </summary>
#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
    [Serializable]
#endif
    public class InvalidOrderFieldException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fieldName">Order field name.</param>
        public InvalidOrderFieldException(string fieldName) :
            base(string.Format(Properties.Strings.OrderByFieldIsNotSupported, fieldName))
        {
        }

#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
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
}
