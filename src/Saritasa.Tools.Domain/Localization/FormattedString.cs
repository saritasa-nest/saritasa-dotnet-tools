using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Formatted string. Constructed by <see cref="StringLocalizerExtensions"/>.
/// See also <seealso href="https://github.com/saritasa-nest/saritasa-dotnet-tools/wiki/Domain-Localization"/>.
/// </summary>
public class FormattedString : IEquatable<FormattedString?>
{
    private readonly string neutralValue;
    private readonly object[] args;

    /// <inheritdoc cref="StronglyTypedFormattedString(Func{string}, object[])" />.
    public static FormattedString StronglyTypedResource(
        Func<string> stronglyTypedResourceLookup,
        params object[] args)
    {
        return new StronglyTypedFormattedString(stronglyTypedResourceLookup, args);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="value">Literal value. <see cref="ToString"/> should return this exact string.</param>
    /// <param name="args">Formattable arguments.</param>
    protected FormattedString(string value, object[]? args)
    {
        this.args = args ??= [];
        neutralValue = args.Length != 0 ? string.Format(value, args) : value;
    }

    /// <summary>
    /// Translate formatted message with current culture settings.
    /// </summary>
    /// <param name="stringLocalizerFactory">String localizer factory.</param>
    public string Localize(IStringLocalizerFactory stringLocalizerFactory)
    {
        var arguments = args?
            .Select(arg => arg switch
            {
                FormattedString fs => fs.Localize(stringLocalizerFactory),
                var obj => obj,
            })
            .ToArray() ?? [];

        return LocalizeInternal(stringLocalizerFactory, arguments);
    }

    /// <summary>
    /// Translate formatted message with current culture settings.
    /// </summary>
    /// <param name="stringLocalizerFactory">String localizer factory.</param>
    /// <param name="arguments">
    /// For every argument of type <see cref="FormattedString"/> passed in a constructor,
    /// there is an already localized string representation.
    /// <see cref="Localize(IStringLocalizerFactory)"/> perform a recursive call to <paramref name="arguments"/>.
    /// </param>
    protected virtual string LocalizeInternal(IStringLocalizerFactory stringLocalizerFactory, object[] arguments)
    {
        return string.Format(neutralValue, arguments);
    }

    /// <summary>
    /// Implicit conversion operator.
    /// </summary>
    /// <param name="value">Literal string value.</param>
    public static implicit operator FormattedString(string value) => new FormattedString(value, null);

    /// <inheritdoc />
    public override string ToString() => neutralValue;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as FormattedString);

    /// <inheritdoc />
    public bool Equals(FormattedString? other)
        => other is not null && EqualityComparer<string>.Default.Equals(ToString(), other.ToString());

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
        return HashCode.Combine(ToString());
#else
        var hashCode = -2109979386;
        return hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ToString());
#endif
    }
}
