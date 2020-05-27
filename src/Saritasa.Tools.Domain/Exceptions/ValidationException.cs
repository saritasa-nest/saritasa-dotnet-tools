// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// Validation exception. Can be mapped to 400 HTTP status code.
    /// </summary>
    [Serializable]
    public class ValidationException : DomainException
    {
        /// <summary>
        /// Validation message formatter. <see cref="ValidationErrorsFormatter.SummaryOrDefaultMessageFormatter" /> by default.
        /// </summary>
        public static ValidationErrorsFormatter.ValidationErrorsMessageFormatter MessageFormatter { get; set; } =
            ValidationErrorsFormatter.SummaryOrDefaultMessageFormatter;

        /// <summary>
        /// Errors dictionary. Key is a member name, value is an enumerable of error
        /// messages. Empty member name relates to summary error message.
        /// For example:
        /// - Name: Field is required.
        /// - ConfirmPassword: Field is required, Should equal to Password field.
        /// </summary>
        public ValidationErrors Errors { get; } = new ValidationErrors();

        /// <inheritdoc />
        public override string Message => MessageFormatter(base.Message, this.Errors);

        /// <summary>
        /// Constructor.
        /// </summary>
        public ValidationException() : base(DomainErrorDescriber.Default.ValidationErrors())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="code">Optional description code for this exception.</param>
        public ValidationException(int code) : base(DomainErrorDescriber.Default.ValidationErrors(), code)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public ValidationException(string message, int code) : base(message, code)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public ValidationException(string message, string code) : base(message, code)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="code">Optional description code for this exception.</param>
        public ValidationException(string message, Exception innerException, int code) :
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
        public ValidationException(string message, Exception innerException, string code) :
            base(message, innerException, code)
        {
        }

        /// <summary>
        /// Constructor with dictionary contain member field as key and error message as value.
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
                this.Errors[error.Key] = new[] { error.Value };
            }
        }

        /// <summary>
        /// Constructor with dictionary contain member field as key and error messages as value.
        /// </summary>
        /// <param name="errors">Member errors dictionary.</param>
        public ValidationException(IDictionary<string, ICollection<string>> errors) :
            base(DomainErrorDescriber.Default.ValidationErrors())
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            this.Errors.Merge(errors);
        }

        /// <summary>
        /// Constructor with dictionary contain member field as key and error messages as value.
        /// </summary>
        /// <param name="errors">Member errors dictionary.</param>
        public ValidationException(IDictionary<string, IEnumerable<string>> errors) :
            base(DomainErrorDescriber.Default.ValidationErrors())
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            this.Errors.Merge(errors.ToDictionary(k => k.Key, v => (ICollection<string>)v.Value));
        }

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var xml = info.GetString("errors");
            if (!string.IsNullOrEmpty(xml))
            {
                var xelement = System.Xml.Linq.XElement.Parse(xml);
                var errorsElements = xelement.Descendants("error").ToDictionary(
                    x => (string) x.Attribute("id"),
                    x => x.Elements("msg").Select(e => e.Value).ToList()
                );
                foreach (var error in errorsElements)
                {
                    Errors.Add(error.Key, error.Value);
                }
            }
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (Errors.Count > 0)
            {
                var xelement = new System.Xml.Linq.XElement(
                    "errors",
                    Errors.Select(x => new System.Xml.Linq.XElement("error",
                        new System.Xml.Linq.XAttribute("id", x.Key),
                        x.Value.Select(e => new System.Xml.Linq.XElement("msg", e))))
                );
                info.AddValue("errors", xelement.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
            }
        }

        /// <summary>
        /// If object contains validation errors throws instance of <see cref="ValidationException" />.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <param name="items">A dictionary of key/value pairs to make available to the service consumers.
        /// This parameter is optional.</param>
        /// <param name="serviceProvider">The object that implements the <see cref="IServiceProvider" /> interface.
        /// This parameter is optional and is not used for netstandard 2.0 .</param>
        public static void ThrowFromObjectValidation(
            object obj,
            IServiceProvider serviceProvider = null,
            IDictionary<object, object> items = null)
        {
            var validationErrors = ValidationErrors.CreateFromObjectValidation(obj, items, serviceProvider);
            if (validationErrors.HasErrors)
            {
                throw new ValidationException(validationErrors);
            }
        }
    }
}
