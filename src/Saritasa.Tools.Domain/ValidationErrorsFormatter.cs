// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Validation message delegates.
    /// </summary>
    public static class ValidationErrorsFormatter
    {
        /// <summary>
        /// Validation message formatter delegate that is used for error text formatting.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationErrors">Validation errors.</param>
        /// <returns>Validation message.</returns>
        public delegate string ValidationErrorsMessageFormatter(string defaultMessage, ValidationErrors validationErrors);

        /// <summary>
        /// Returns summary message if a specific key exists or defaults one.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationErrors">Validation errors.</param>
        /// <returns>Validation message.</returns>
        public static string SummaryOrDefaultMessageFormatter(string defaultMessage, ValidationErrors validationErrors)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationErrors == null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (validationErrors.ContainsKey(ValidationErrors.SummaryKey))
            {
                return validationErrors[ValidationErrors.SummaryKey].First();
            }
            return defaultMessage;
        }

        /// <summary>
        /// Returns the first available validation error. If no errors exist just return the default message.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationErrors">Validation errors.</param>
        /// <returns>Validation message.</returns>
        public static string FirstErrorOrDefaultMessageFormatter(string defaultMessage, ValidationErrors validationErrors)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationErrors == null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (validationErrors.Any())
            {
                return validationErrors.First().Value.First();
            }
            return defaultMessage;
        }

        /// <summary>
        /// Group messages by fields. Example:
        /// Summary message.
        /// - Field1: Validation message 1. Validation message 2.
        /// - Field2: Validation message.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationErrors">Validation errors.</param>
        /// <returns>Validation message.</returns>
        public static string GroupErrorsOrDefaultMessageFormatter(string defaultMessage, ValidationErrors validationErrors)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationErrors == null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            const string separator = " ";
            if (validationErrors.Any())
            {
                var sb = new StringBuilder(validationErrors.Count * 55);
                foreach (KeyValuePair<string, ICollection<string>> errorMember in validationErrors.OrderBy(e => e.Key))
                {
                    if (errorMember.Key.Equals(ValidationErrors.SummaryKey))
                    {
                        sb.AppendLine(string.Join(separator, errorMember.Value));
                    }
                    else
                    {
                        sb.AppendLine($"- {errorMember.Key}: {string.Join(separator, errorMember.Value)}");
                    }
                }
                return sb.ToString();
            }
            return defaultMessage;
        }

        /// <summary>
        /// List error messages on separate lines. Example:
        /// Summary message.
        /// Validation message 1.
        /// Validation message 2.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationErrors">Validation errors.</param>
        /// <returns>Validation message.</returns>
        public static string ListErrorsOrDefaultMessageFormatter(string defaultMessage, ValidationErrors validationErrors)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationErrors == null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (validationErrors.Any())
            {
                var sb = new StringBuilder(validationErrors.Count * 55);
                foreach (KeyValuePair<string, ICollection<string>> errorMember in validationErrors)
                {
                    sb.AppendLine(errorMember.Key);
                }
                return sb.ToString();
            }
            return defaultMessage;
        }
    }
}
