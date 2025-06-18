// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.Extensions.Localization;
using Saritasa.Tools.Domain.Exceptions;

namespace Saritasa.Tools.Domain;

/// <summary>
/// Validation message delegates.
/// </summary>
public static class ValidationErrorsFormatter
{
    private readonly struct DummyStringLocalizer : IStringLocalizer
    {
        public static IStringLocalizer Instance { get; } = default(DummyStringLocalizer);

        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, value: string.Format(name, arguments));

        IEnumerable<LocalizedString> IStringLocalizer.GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
    }

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
        var formatter = SummaryOrDefaultMessageFormatter(DummyStringLocalizer.Instance);
        return formatter(defaultMessage, validationErrors);
    }

    /// <summary>
    /// Returns summary message if a specific key exists or defaults one.
    /// </summary>
    /// <param name="localizer">Localizer.</param>
    public static ValidationErrorsMessageFormatter SummaryOrDefaultMessageFormatter(IStringLocalizer localizer)
    {
        return (defaultMessage, validationErrors) =>
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
                var formattedMessage = validationErrors[ValidationErrors.SummaryKey].First();
                return localizer.Format(formattedMessage);
            }
            return defaultMessage;
        };
    }

    /// <summary>
    /// Returns the first available validation error. If no errors exist just return the default message.
    /// </summary>
    /// <param name="defaultMessage">Default message.</param>
    /// <param name="validationErrors">Validation errors.</param>
    /// <returns>Validation message.</returns>
    public static string FirstErrorOrDefaultMessageFormatter(string defaultMessage, ValidationErrors validationErrors)
    {
        var formatter = FirstErrorOrDefaultMessageFormatter(DummyStringLocalizer.Instance);
        return formatter(defaultMessage, validationErrors);
    }

    /// <summary>
    /// Returns the first available validation error. If no errors exist just return the default message.
    /// </summary>
    /// <param name="localizer">Localizer.</param>
    public static ValidationErrorsMessageFormatter FirstErrorOrDefaultMessageFormatter(IStringLocalizer localizer)
    {
        return (defaultMessage, validationErrors) =>
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
                var formattedMessage = validationErrors.First().Value.First();
                return localizer.Format(formattedMessage);
            }
            return defaultMessage;
        };
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
        var formatter = GroupErrorsOrDefaultMessageFormatter(DummyStringLocalizer.Instance);
        return formatter(defaultMessage, validationErrors);
    }

    /// <summary>
    /// Group messages by fields. Example:
    /// Summary message.
    /// - Field1: Validation message 1. Validation message 2.
    /// - Field2: Validation message.
    /// </summary>
    /// <param name="localizer">Localizer.</param>
    public static ValidationErrorsMessageFormatter GroupErrorsOrDefaultMessageFormatter(IStringLocalizer localizer)
    {
        return (defaultMessage, validationErrors) =>
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
                foreach (KeyValuePair<string, ICollection<FormattedString>> errorMember in validationErrors.OrderBy(e => e.Key))
                {
                    var formatted = string.Join(separator, errorMember.Value.Select(localizer.Format));
                    if (errorMember.Key.Equals(ValidationErrors.SummaryKey))
                    {
                        sb.AppendLine(formatted);
                    }
                    else
                    {
                        sb.AppendLine($"- {errorMember.Key}: {formatted}");
                    }
                }
                return sb.ToString();
            }
            return defaultMessage;
        };
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
            foreach (KeyValuePair<string, ICollection<FormattedString>> errorMember in validationErrors)
            {
                sb.AppendLine(errorMember.Key);
            }
            return sb.ToString();
        }
        return defaultMessage;
    }
}
