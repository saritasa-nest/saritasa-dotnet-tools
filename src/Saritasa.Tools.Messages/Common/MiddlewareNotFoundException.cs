// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Middleware not found exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MiddlewareNotFoundException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public MiddlewareNotFoundException() : base(Properties.Strings.MiddlewareNotFound)
        {
        }

#if NET452
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected MiddlewareNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
