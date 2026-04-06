#if NET5_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Saritasa.Tools.Domain.Properties;

namespace Saritasa.Tools.Domain.Attributes;

/// <summary>
/// Email address validation rule that aligns with RFC 2822 Section 3.4.1 (no display name allowed).
/// Compatible with <see cref="MailAddress"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RfcEmailAddressAttribute : DataTypeAttribute
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public RfcEmailAddressAttribute()
        : base(DataType.EmailAddress)
    {
        ErrorMessageResourceType = typeof(Strings);
        ErrorMessageResourceName = nameof(Strings.RfcEmailAttribute_Invalid);
    }

    /// <inheritdoc/>
    public override bool IsValid(object? value)
    {
        return value is not string valueAsString
            || MailAddress.TryCreate(valueAsString, displayName: null, out _);
    }
}
#endif
