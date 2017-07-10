// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Domain.Exceptions
{
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
        public IDictionary<string, IEnumerable<string>> Errors
        {
            get => errors;
        }

        private readonly IDictionary<string, IEnumerable<string>> errors
            = new Dictionary<string, IEnumerable<string>>();

        /// <inheritdoc />
        public override string Message
        {
            get
            {
                if (Errors.ContainsKey(SummaryKey))
                {
                    return Errors[SummaryKey].First();
                }
                return base.Message;
            }
        }

        /// <summary>
        /// Summary errors. Returns zero array if not defined.
        /// </summary>
        public IEnumerable<string> SummaryErrors
        {
            get => errors.ContainsKey(SummaryKey) ? errors[SummaryKey] : new string[0];
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ValidationException() : base(DomainErrorDescriber.Default.ValidationErrors())
        {
        }

        /// <summary>
        /// .ctor with message.
        /// <param name="message">Exception message.</param>
        /// </summary>
        public ValidationException(string message) : base(message)
        {
            AddError(message);
        }

        /// <summary>
        /// .ctor with message and inner exception.
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        /// </summary>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            AddError(message);
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

        /// <summary>
        /// Add error to errors list for specific member.
        /// </summary>
        /// <param name="member">Member of field name. Can be empty.</param>
        /// <param name="error">Error message.</param>
        public void AddError(string member, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                throw new ArgumentException("Error cannot be empty.", nameof(error));
            }

            var list = errors.ContainsKey(member) ? (IList<string>)errors[member] : new List<string>();
            list.Add(error);
        }

        /// <summary>
        /// Add summar error.
        /// </summary>
        /// <param name="error">Error message.</param>
        public void AddError(string error)
        {
            AddError(SummaryKey, error);
        }

#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
        /// <summary>
        /// Creates on throws instance of <see cref="ValidationException" /> based on validation results
        /// of <see cref="IValidatableObject" /> object.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <param name="serviceProvider">The object that implements the <see cref="IServiceProvider" /> interface.
        /// This parameter is optional.</param>
        /// <param name="items">A dictionary of key/value pairs to make available to the service consumers.
        /// This parameter is optional.</param>
        public static void ThrowFromObjectValidation(
            IValidatableObject obj,
            IServiceProvider serviceProvider = null,
            IDictionary<object, object> items = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var result = obj.Validate(new ValidationContext(obj, serviceProvider, items)).ToArray();
            if (result.Any())
            {
                var ex = new ValidationException();
                foreach (ValidationResult resultItem in result)
                {
                    var member = resultItem.MemberNames.Any() ? resultItem.MemberNames.First() : string.Empty;
                    ex.AddError(member, resultItem.ErrorMessage);
                }
                throw ex;
            }
        }
#endif
    }
}
