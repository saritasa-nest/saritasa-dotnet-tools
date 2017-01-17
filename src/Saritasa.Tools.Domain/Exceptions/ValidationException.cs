// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain.Exceptions
{
    using System;
    using System.Collections.Generic;
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    using System.Runtime.Serialization;
#endif

    /// <summary>
    /// Validation exception. Can be mapped to 400 HTTP status code.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class ValidationException : DomainException
    {
        const string SummaryKey = "";

        /// <summary>
        /// Errors dictionary. Empty string key relates to summary error message.
        /// </summary>
        public IDictionary<string, string> Errors = new Dictionary<string, string>();

        /// <inheritdoc />
        public override string Message
        {
            get
            {
                if (Errors.ContainsKey(SummaryKey))
                {
                    return Errors[SummaryKey];
                }
                return base.Message;
            }
        }

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
            Errors[SummaryKey] = message;
        }

        /// <summary>
        /// .ctor with message and inner exception.
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        /// </summary>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            Errors[SummaryKey] = message;
        }

#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
        /// <summary>
        /// .ctor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
