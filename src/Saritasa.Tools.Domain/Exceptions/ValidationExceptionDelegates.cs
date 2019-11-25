// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saritasa.Tools.Domain.Exceptions
{
    /// <summary>
    /// Validation message delegates.
    /// </summary>
    public static class ValidationExceptionDelegates
    {
        /// <summary>
        /// Returns summary message if specific key exists or default one.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationException">Validation exception.</param>
        /// <returns>Validation message.</returns>
        public static string SummaryOrDefaultMessageFormatter(string defaultMessage, ValidationException validationException)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationException == null)
            {
                throw new ArgumentNullException(nameof(validationException));
            }

            if (validationException.Errors.ContainsKey(ValidationException.SummaryKey))
            {
                return validationException.Errors[ValidationException.SummaryKey].First();
            }
            return defaultMessage;
        }

        /// <summary>
        /// Returns first available validation error. If no errors exist just return default message.
        /// </summary>
        /// <param name="defaultMessage">Default message.</param>
        /// <param name="validationException">Validation exception.</param>
        /// <returns>Validation message.</returns>
        public static string FirstErrorOrDefaultMessageFormatter(string defaultMessage, ValidationException validationException)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationException == null)
            {
                throw new ArgumentNullException(nameof(validationException));
            }

            if (validationException.Errors.Any())
            {
                return validationException.Errors.First().Value.First();
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
        /// <param name="validationException">Validation exception.</param>
        /// <returns>Validation message.</returns>
        public static string GroupErrorsOrDefaultMessageFormatter(string defaultMessage, ValidationException validationException)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationException == null)
            {
                throw new ArgumentNullException(nameof(validationException));
            }

            const string separator = " ";
            if (validationException.Errors.Any())
            {
                var sb = new StringBuilder(validationException.Errors.Count * 55);
                foreach (KeyValuePair<string, ICollection<string>> errorMember in validationException.Errors.OrderBy(e => e.Key))
                {
                    if (errorMember.Key.Equals(ValidationException.SummaryKey))
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
        /// <param name="validationException">Validation exception.</param>
        /// <returns>Validation message.</returns>
        public static string ListErrorsOrDefaultMessageFormatter(string defaultMessage, ValidationException validationException)
        {
            if (string.IsNullOrEmpty(defaultMessage))
            {
                throw new ArgumentNullException(nameof(defaultMessage));
            }
            if (validationException == null)
            {
                throw new ArgumentNullException(nameof(validationException));
            }

            if (validationException.Errors.Any())
            {
                var sb = new StringBuilder(validationException.Errors.Count * 55);
                foreach (KeyValuePair<string, ICollection<string>> errorMember in validationException.Errors)
                {
                    sb.AppendLine(errorMember.Key);
                }
                return sb.ToString();
            }
            return defaultMessage;
        }
    }
}
