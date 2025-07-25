using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Formatted string. Consists of localizable resource string and its arguments.
/// See also <seealso href="https://github.com/saritasa-nest/saritasa-dotnet-tools/wiki/Domain-Localization"/>.
/// </summary>
public class FormattedString : IEquatable<FormattedString?>
{
    private readonly string neutralValue;
    private readonly object[] args;

    /// <inheritdoc cref="StronglyTypedFormattedString(Func{string}, object[])" />.
    public static FormattedString FromResources(
        Func<string> stronglyTypedResourceLookup,
        params object[] args)
    {
        return new StronglyTypedFormattedString(stronglyTypedResourceLookup, args);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="neutralFormat">Neutral format value.</param>
    /// <param name="args">Formattable arguments.</param>
    /// <remarks><see cref="ToString()"/> should be constructed from both <paramref name="neutralFormat"/> and <paramref name="args"/>.</remarks>
    protected FormattedString(string neutralFormat, object[]? args)
    {
        this.args = args ??= [];
        neutralValue = args.Length != 0 ? string.Format(neutralFormat, args) : neutralFormat;
    }

    /// <summary>
    /// Translate formatted message with current culture settings.
    /// </summary>
    /// <param name="stringLocalizerFactory">String localizer factory.</param>
    public string Localize(IStringLocalizerFactory stringLocalizerFactory)
    {
        var arguments = args
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
    /// <param name="localizedArguments">
    /// For every argument of type <see cref="FormattedString"/> passed in a constructor,
    /// there is an already localized string representation.
    /// <see cref="Localize(IStringLocalizerFactory)"/> perform a recursive call to <paramref name="localizedArguments"/>.
    /// </param>
    protected virtual string LocalizeInternal(IStringLocalizerFactory stringLocalizerFactory, object[] localizedArguments)
    {
        // The base class aggregates only neutral (erased) version of the string, so it is not localizable by design.
        // This method must be overriden by all of the subclasses.
        return ToString();
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
