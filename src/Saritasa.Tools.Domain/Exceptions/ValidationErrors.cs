// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// The collection of key-value validation messages. Allows to accumulate
    /// errors per key.
    /// </summary>
    public class ValidationErrors : Dictionary<string, ICollection<string>>
    {
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
        /// Add error to errors list for specific key.
        /// </summary>
        /// <param name="key">Member of field name or key. Can be empty.</param>
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
        /// Add summary error.
        /// </summary>
        /// <param name="error">Error message.</param>
        public void AddError(string error)
        {
            AddError(ValidationException.SummaryKey, error);
        }

        /// <summary>
        /// Returns dictionary that contains only one first error message per member name.
        /// </summary>
        /// <returns>Member error dictionary.</returns>
        public IDictionary<string, string> GetOneErrorDictionary()
        {
            return this.ToDictionary(k => k.Key, v => v.Value.FirstOrDefault() ?? string.Empty);
        }

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
    }
}
