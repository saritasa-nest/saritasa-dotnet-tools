using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Formatted string. Constructed by <see cref="StringLocalizerExtensions"/>.
/// See also <seealso href="https://github.com/saritasa-nest/saritasa-dotnet-tools/wiki/Domain-Localization"/>.
/// </summary>
public class FormattedString
{
    /// <summary>
    /// Formatting pattern as localized string.
    /// Contains the name of the resource and the neutral value.
    /// </summary>
    public LocalizedString Format { get; }

    /// <summary>
    /// Formattable Arguments.
    /// </summary>
    public object[]? Arguments { get; }

    /// <summary>
    /// Resource provider type.
    /// </summary>
    internal Type? ResourceType { get; }

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
    /// <param name="resourceType">Resource provider type.</param>
    /// <param name="format">Localized format.</param>
    /// <param name="args">Formattable arguments.</param>
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
    public override string ToString()
        => Arguments is null ? Format : string.Format(Format, Arguments);
}

/// <summary>
/// Generic class for formatted string.
/// Reuses the same ResourceType object per the closed generic class.
/// </summary>
/// <typeparam name="TResource">Resource type.</typeparam>
/// <param name="format">Localized format.</param>
/// <param name="args">Formattable arguments.</param>
internal class FormattedString<TResource>(LocalizedString format, object[] args) :
    FormattedString(type, format, args)
{
    private static readonly Type type = typeof(TResource);
}
