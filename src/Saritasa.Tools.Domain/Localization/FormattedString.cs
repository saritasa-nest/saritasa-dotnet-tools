using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Formatted string.
/// </summary>
public class FormattedString
{
    public LocalizedString Format { get; }

    public Type? ResourceType { get; }

    public object[]? Arguments { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="value">Literal value.</param>
    public FormattedString(string value)
    {
        Format = new(value, value, resourceNotFound: true);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="format">Literal localized value.</param>
    protected FormattedString(
        Type resourceType,
        LocalizedString format,
        object[] args)
    {
        ResourceType = resourceType;
        Format = format;
        Arguments = args;
    }

    /// <summary>
    /// Implicit conversion operator.
    /// </summary>
    /// <param name="value">Literal string value.</param>
    public static implicit operator FormattedString(string value) => new FormattedString(value);

    /// <inheritdoc />
    public override string ToString() => string.Format(Format, Arguments);
}

internal class FormattedString<T>(LocalizedString format, object[] args) :
    FormattedString(type, format, args)
{
    private static readonly Type type = typeof(T);
}
