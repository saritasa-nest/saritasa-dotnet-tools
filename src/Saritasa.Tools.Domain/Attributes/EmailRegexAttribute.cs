using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Saritasa.Tools.Domain.Properties;

namespace Saritasa.Tools.Domain.Attributes;

/// <summary>
/// Email address validation attribute that uses regular expression.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class EmailRegexAttribute : DataTypeAttribute
{
    private const string EmailValidFormat = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";

    /// <summary>
    /// Constructor.
    /// </summary>
    public EmailRegexAttribute()
        : base(DataType.EmailAddress)
    {
        ErrorMessageResourceType = typeof(Strings);
        ErrorMessageResourceName = nameof(Strings.EmailRegexAttribute_Invalid);
    }

    /// <inheritdoc/>
    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is string valueAsString)
        {
            return Regex.IsMatch(valueAsString, EmailValidFormat);
        }

        return false;
    }
}
