// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
#if NET40
using System.Runtime.Serialization;
#endif

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// Validation exception. Can be mapped to 400 HTTP status code.
    /// </summary>
#if NET40
    [Serializable]
#endif
    public class ValidationException : DomainException
    {
        const string SummaryKey = "";

        /// <summary>
        /// Errors dictionary. Key is a member name, value is an enumerable of error
        /// messages. Empty member name relates to summary error message.
        /// </summary>
        public virtual IDictionary<string, IEnumerable<string>> Errors
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

        /// <summary>
        /// .ctor with dictionary contain member field as key and error message as value.
        /// </summary>
        /// <param name="errors">Member error dictionary.</param>
        public ValidationException(IDictionary<string, string> errors) :
            base(DomainErrorDescriber.Default.ValidationErrors())
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            foreach (var error in errors)
            {
                this.errors[error.Key] = new string[] { error.Value };
            }
        }

        /// <summary>
        /// .ctor with dictionary contain member field as key and error messages as value.
        /// </summary>
        /// <param name="errors">Member errors dictionary.</param>
        public ValidationException(IDictionary<string, IEnumerable<string>> errors) :
            base(DomainErrorDescriber.Default.ValidationErrors())
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            this.errors = errors;
        }

#if NET40
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
                throw new ArgumentException(DomainErrorDescriber.Default.ValidationErrorIsEmpty(), nameof(error));
            }

            IEnumerable<string> list;
            if (errors.TryGetValue(member, out list))
            {
                ((IList<string>)list).Add(error);
            }
            else
            {
                list = new List<string>();
                ((IList<string>)list).Add(error);
                errors.Add(member, list);
            }
        }

        /// <summary>
        /// Add summar error.
        /// </summary>
        /// <param name="error">Error message.</param>
        public void AddError(string error)
        {
            AddError(SummaryKey, error);
        }

        /// <summary>
        /// Returns opposite dictionary where key is error message and value is an
        /// enumerable with member names related to the error.
        /// </summary>
        /// <returns>Error members dictionary.</returns>
        public IDictionary<string, IEnumerable<string>> GetErrorMembersDictionary()
        {
            var dict = new Dictionary<string, IEnumerable<string>>();
            foreach (KeyValuePair<string, IEnumerable<string>> member in errors)
            {
                foreach (string error in member.Value)
                {
                    IEnumerable<string> list;
                    if (dict.TryGetValue(error, out list))
                    {
                        ((IList<string>)list).Add(member.Key);
                    }
                    else
                    {
                        list = new List<string>();
                        ((IList<string>)list).Add(member.Key);
                        dict.Add(error, list);
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Returns dictionary that contains only one first error message per member name.
        /// </summary>
        /// <returns>Member error dictionary.</returns>
        public IDictionary<string, string> GetOneErrorDictionary()
        {
            return errors.ToDictionary(k => k.Key, v => v.Value.FirstOrDefault() ?? string.Empty);
        }

#if NET40
        /// <summary>
        /// Creates on throws instance of <see cref="ValidationException" /> based on validation results
        /// of object.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <param name="serviceProvider">The object that implements the <see cref="IServiceProvider" /> interface.
        /// This parameter is optional.</param>
        /// <param name="items">A dictionary of key/value pairs to make available to the service consumers.
        /// This parameter is optional.</param>
        public static void ThrowFromObjectValidation(
            object obj,
            IServiceProvider serviceProvider = null,
            IDictionary<object, object> items = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var validationResults = new List<ValidationResult>();
            var result = Validator.TryValidateObject(obj, new ValidationContext(obj, serviceProvider, items),
                validationResults, true);
            if (!result)
            {
                var ex = new ValidationException();
                foreach (ValidationResult validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        ex.AddError(memberName, validationResult.ErrorMessage);
                    }
                }
                throw ex;
            }
        }
#else
        /// <summary>
        /// Creates on throws instance of <see cref="ValidationException" /> based on validation results
        /// of object.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <param name="items">A dictionary of key/value pairs to make available to the service consumers.
        /// This parameter is optional.</param>
        public static void ThrowFromObjectValidation(
            object obj,
            IDictionary<object, object> items = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var validationResults = new List<ValidationResult>();
            var result = Validator.TryValidateObject(obj, new ValidationContext(obj, items),
                validationResults, true);
            if (!result)
            {
                var ex = new ValidationException();
                foreach (ValidationResult validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        ex.AddError(memberName, validationResult.ErrorMessage);
                    }
                }
                throw ex;
            }
        }
#endif
    }
}
