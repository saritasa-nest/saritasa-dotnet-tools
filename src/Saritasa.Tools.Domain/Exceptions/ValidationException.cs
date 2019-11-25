// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <summary>
        /// Default summary validation key. Should contain overall message.
        /// </summary>
        internal const string SummaryKey = "";

        /// <summary>
        /// Validation message formatter delegate used for Message property output.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationException">Validation exception.</param>
        /// <returns>Validation message.</returns>
        /// <remarks>
        /// Do not use validationException.Message property because it leads to recursion.
        /// </remarks>
        public delegate string ValidationMessageFormatter(string defaultMessage, ValidationException validationException);

        /// <summary>
        /// Validation message formatter. <see cref="ValidationExceptionDelegates.SummaryOrDefaultMessageFormatter" /> by default.
        /// </summary>
        public static ValidationMessageFormatter MessageFormatter { get; set; } = ValidationExceptionDelegates.SummaryOrDefaultMessageFormatter;

        /// <summary>
        /// Errors dictionary. Key is a member name, value is an enumerable of error
        /// messages. Empty member name relates to summary error message.
        /// For example:
        /// - Name: Field is required.
        /// - ConfirmPassword: Field is required, Should equal to Password field.
        /// </summary>
        public ValidationErrors Errors { get; } = new ValidationErrors();

        /// <inheritdoc />
        public override string Message => MessageFormatter(base.Message, this);

        /// <summary>
        /// Summary errors. Returns zero array if not defined.
        /// </summary>
        public IEnumerable<string> SummaryErrors => Errors.ContainsKey(SummaryKey) ?
            Errors[SummaryKey] : Enumerable.Empty<string>();

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

#if NET40
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

            if (UseMetadataType)
            {
                RegisterMetadataType(obj.GetType());
            }

            var validationResults = new List<ValidationResult>();
            var result = Validator.TryValidateObject(obj, new ValidationContext(obj, serviceProvider, items),
                validationResults, true);
            if (!result)
            {
                var ex = new ValidationException();
                foreach (ValidationResult validationResult in validationResults)
                {
                    if (validationResult.MemberNames != null && validationResult.MemberNames.Any())
                    {
                        foreach (var memberName in validationResult.MemberNames)
                        {
                            ex.Errors.AddError(memberName, validationResult.ErrorMessage);
                        }
                    }
                    else
                    {
                        ex.Errors.AddError(validationResult.ErrorMessage);
                    }
                }
                throw ex;
            }
        }

        /// <summary>
        /// If <c>true</c> there will be additional check for <see cref="MetadataTypeAttribute" />
        /// and metadata type registration. <c>False</c> by default.
        /// </summary>
        public static bool UseMetadataType { get; set; } = false;

        private static readonly ConcurrentDictionary<Type, bool> registeredMetadataType =
            new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// For some reason if type base <see cref="MetadataTypeAttribute" /> type that contains
        /// validation attributes they will not be taken into account. This is workaround that
        /// registers them explicitly.
        /// </summary>
        /// <param name="t">Type to register.</param>
        private static void RegisterMetadataType(Type t)
        {
            var attributes = t.GetCustomAttributes(typeof(MetadataTypeAttribute), true);
            if (attributes.Length > 0)
            {
                registeredMetadataType.GetOrAdd(t, type =>
                {
                    foreach (MetadataTypeAttribute attribute in attributes)
                    {
                        TypeDescriptor.AddProviderTransparent(
                            new AssociatedMetadataTypeTypeDescriptionProvider(type, attribute.MetadataClassType), type);
                    }
                    return true;
                });
            }
        }
#else
        /// <summary>
        /// If object contains validation errors throws instance of <see cref="ValidationException" />.
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
                    if (validationResult.MemberNames != null && validationResult.MemberNames.Any())
                    {
                        foreach (var memberName in validationResult.MemberNames)
                        {
                            ex.Errors.AddError(memberName, validationResult.ErrorMessage);
                        }
                    }
                    else
                    {
                        ex.Errors.AddError(validationResult.ErrorMessage);
                    }
                }
                throw ex;
            }
        }
#endif
    }
}
