using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Exceptions;

/// <summary>
/// Formatted string.
/// </summary>
/// <param name="Format">Localized format.</param>
/// <param name="Arguments">Arguments.</param>
public record struct FormattedString(
    LocalizedString Format,
    params object[]? Arguments)
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="value">Literal value.</param>
    public FormattedString(string value) : this(new LocalizedString(value, value))
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="format">Literal localized value.</param>
    public FormattedString(LocalizedString format) : this(format, null)
    {
    }

    /// <summary>
    /// Implicit conversion operator.
    /// </summary>
    /// <param name="value">Literal string value.</param>
    public static implicit operator FormattedString(string value) => new FormattedString(value);

    /// <inheritdoc />
    public override readonly string ToString() => string.Format(Format, Arguments);
}
