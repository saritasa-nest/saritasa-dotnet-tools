using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Formatted string. Constructed by <see cref="StringLocalizerExtensions"/>.
/// See also <seealso href="https://github.com/saritasa-nest/saritasa-dotnet-tools/wiki/Domain-Localization"/>.
/// </summary>
public class FormattedString : IEquatable<FormattedString?>
{
    private readonly string neuterValue;
    private readonly object[] args;

    public static FormattedString StronglyTypedResource(Func<string> resolve, params object[] args)
    {
        return new StronglyTypedFormattedString(resolve, args);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="value">Literal value.</param>
    protected FormattedString(string value, object[]? args)
    {
        args ??= [];

        this.args = args;
        this.neuterValue = args.Length != 0 ? string.Format(value, args) : value;
    }

    public string Localize(IStringLocalizerFactory stringLocalizerFactory)
    {
        var arguments = args?.Select(arg => arg switch
        {
            FormattedString fs => fs.Localize(stringLocalizerFactory),
            var obj => obj,
        }).ToArray() ?? [];

        return LocalizeInternal(stringLocalizerFactory, arguments);
    }

    protected virtual string LocalizeInternal(IStringLocalizerFactory localizerFactory, object[] arguments)
    {
        return string.Format(neuterValue, arguments);
    }

    /// <summary>
    /// Implicit conversion operator.
    /// </summary>
    /// <param name="value">Literal string value.</param>
    public static implicit operator FormattedString(string value) => new FormattedString(value, null);

    /// <inheritdoc />
    public override string ToString() => neuterValue;

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
