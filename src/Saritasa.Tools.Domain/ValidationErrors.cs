// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// The collection of key-value validation messages. Allows accumulating
    /// errors per key.
    /// </summary>
    [DebuggerDisplay("{Count} errors: {ErrorsKeys}")]
    public class ValidationErrors : Dictionary<string, ICollection<string>>
    {
        /// <summary>
        /// Default summary validation key. Should contain the overall message.
        /// </summary>
        public const string SummaryKey = "";

        /// <summary>
        /// Returns <c>true</c> if there were any errors added.
        /// </summary>
        public bool HasErrors => this.Count > 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ValidationErrors()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="errors">Initial errors to initialize with.</param>
        public ValidationErrors(IDictionary<string, ICollection<string>> errors) : base(errors)
        {
        }

        /// <summary>
        /// Creates <see cref="ValidationErrors" /> instance from data annotation attributes.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <param name="items">The dictionary of key/value pairs to make available to the service consumers.
        /// This parameter is optional.</param>
        /// <param name="serviceProvider">The object that implements the <see cref="IServiceProvider" /> interface.
        /// This parameter is optional and is not used for netstandard 2.0 .</param>
        /// <returns><see cref="ValidationErrors" /> instance.</returns>
        public static ValidationErrors CreateFromObjectValidation(
            object obj,
            IDictionary<object, object>? items = null,
            IServiceProvider? serviceProvider = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

#if NET40 || NET452
            if (UseMetadataType)
            {
                RegisterMetadataType(obj.GetType());
            }
#endif

            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var result = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                obj,
#if NET40 || NET452
                new System.ComponentModel.DataAnnotations.ValidationContext(obj, serviceProvider, items),
#else
                new System.ComponentModel.DataAnnotations.ValidationContext(obj, items),
#endif
                validationResults,
                validateAllProperties: true);
            var validationErrors = new ValidationErrors();
            if (!result)
            {
                foreach (var validationResult in validationResults)
                {
                    if (validationResult.MemberNames != null && validationResult.MemberNames.Any())
                    {
                        foreach (var memberName in validationResult.MemberNames)
                        {
                            validationErrors.AddError(memberName, validationResult.ErrorMessage);
                        }
                    }
                    else
                    {
                        validationErrors.AddError(validationResult.ErrorMessage);
                    }
                }
            }
            return validationErrors;
        }

        #region Metadata Type

#if NET40 || NET452
        /// <summary>
        /// If <c>true</c> there will be additional check for <see cref="System.ComponentModel.DataAnnotations.MetadataTypeAttribute" />
        /// and metadata type registration. <c>False</c> by default.
        /// </summary>
        public static bool UseMetadataType { get; set; } = false;

        private static readonly ConcurrentDictionary<Type, bool> registeredMetadataType =
            new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// For some reason if type base <see cref="System.ComponentModel.DataAnnotations.MetadataTypeAttribute" /> type that contains
        /// validation attributes they will not be taken into account. This is a workaround that
        /// registers them explicitly.
        /// </summary>
        /// <param name="t">Type to register.</param>
        private static void RegisterMetadataType(Type t)
        {
            var attributes = t.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MetadataTypeAttribute), true);
            if (attributes.Length > 0)
            {
                registeredMetadataType.GetOrAdd(t, type =>
                {
                    foreach (System.ComponentModel.DataAnnotations.MetadataTypeAttribute attribute in attributes)
                    {
                        System.ComponentModel.TypeDescriptor.AddProviderTransparent(
                            new System.ComponentModel.DataAnnotations.AssociatedMetadataTypeTypeDescriptionProvider(type, attribute.MetadataClassType), type);
                    }
                    return true;
                });
            }
        }
#endif

        #endregion

        /// <summary>
        /// Create <see cref="ValidationErrors" /> object with the single key and errors.
        /// </summary>
        /// <param name="key">Member of field name or key. It can be empty.</param>
        /// <param name="errors">Error messages.</param>
        /// <returns><see cref="ValidationErrors" /> instance.</returns>
        public static ValidationErrors CreateFromErrors(string key, params string[] errors)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }
            if (errors.Length == 0)
            {
                throw new ArgumentException(Properties.Strings.ValidationErrorIsEmpty, nameof(errors));
            }

            return new ValidationErrors(new Dictionary<string, ICollection<string>>
            {
                [key] = errors
            });
        }

        /// <summary>
        /// Add error to errors list for the specific key.
        /// </summary>
        /// <param name="key">Member of field name or key. It can be empty.</param>
        /// <param name="error">Error message.</param>
        public void AddError(string key, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                throw new ArgumentException(DomainErrorDescriber.Default.ValidationErrorIsEmpty(), nameof(error));
            }

            if (this.TryGetValue(key, out var list))
            {
                list.Add(error);
            }
            else
            {
                list = new List<string>();
                list.Add(error);
                this.Add(key, list);
            }
        }

        /// <summary>
        /// Add a summary error.
        /// </summary>
        /// <param name="error">Error message.</param>
        public void AddError(string error)
        {
            AddError(SummaryKey, error);
        }

        /// <summary>
        /// Returns dictionary that contains only one first error message per member name.
        /// </summary>
        /// <returns>Member error dictionary.</returns>
        public IDictionary<string, string> GetOneErrorDictionary() => this.ToDictionary(k => k.Key, v => v.Value.FirstOrDefault() ?? string.Empty);

        /// <summary>
        /// Merge with another errors dictionary.
        /// </summary>
        /// <param name="dictionary">Errors dictionary to merge with.</param>
        public void Merge(IDictionary<string, ICollection<string>> dictionary)
        {
            foreach (KeyValuePair<string, ICollection<string>> errorKeyValue in dictionary)
            {
                foreach (var errors in errorKeyValue.Value)
                {
                    this.AddError(errorKeyValue.Key, errors);
                }
            }
        }

        /// <summary>
        /// Summary errors. Returns zero enumerable if not defined.
        /// </summary>
        public IEnumerable<string> SummaryErrors => this.ContainsKey(SummaryKey) ? this[SummaryKey] : Enumerable.Empty<string>();

        private string ErrorsKeys => string.Join(", ",
            this.Keys.Select(k => k != SummaryKey ? k : "<summary>"));
    }
}
