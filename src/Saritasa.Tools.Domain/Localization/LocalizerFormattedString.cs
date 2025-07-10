using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Xml.Linq;
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

    /// <summary>
    /// Represent string value as localized string.
    /// </summary>
    /// <remarks>
    /// Usage example.
    /// <example>
    /// <code>
    /// Resources.Strings.EmailNotFound.CreateFromResource&lt;Resources.Strings&gt;();
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="value">Resource value.</param>
    /// <param name="name">Resource name. Should be the same as <see cref="LocalizedString.Name"/>.</param>
    public static FormattedString FromResource<T>(
        string value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        name = name?.Split('.')?.Last();
        return new LocalizerFormattedString(typeof(T), new LocalizedString(name ?? value, value));
    }

    public static FormattedString FromLambda(Func<string> lambda)
    {
        return new LazyFormattedString(lambda);
    }

    public virtual FormattedString WithArgs(params object[] args)
        => new FormattedString(string.Format(neuterValue, args));

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="value">Literal value.</param>
    protected FormattedString(string value)
    {
        neuterValue = value;
        args = [];
    }

    protected FormattedString(FormattedString copy, params object[] args)
        : this(string.Format(copy.neuterValue, args))
    {
        this.args = copy.args.Concat(args).ToArray() ?? [];
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
    public static implicit operator FormattedString(string value) => new FormattedString(value);

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

/// <summary>
/// Generic class for formatted string.
/// Reuses the same ResourceType object per the closed generic class.
/// </summary>
internal class LocalizerFormattedString : FormattedString
{
    private readonly LocalizedString format;
    private readonly Type resourceType;

    /// <param name="format">Formatting pattern as localized string. Contains the name of the resource and the neutral value.</param>
    public LocalizerFormattedString(Type resourceType, LocalizedString format) : base(format)
    {
        this.resourceType = resourceType;
        this.format = format;
    }

    private LocalizerFormattedString(LocalizerFormattedString copy, object[] args) : base(copy, args)
    {
        resourceType = copy.resourceType;
        format = copy.format;
    }

    /// <summary>
    /// Provide arguments to formatted string.
    /// </summary>
    /// <param name="args">Arguments.</param>
    public override FormattedString WithArgs(params object[] args)
        => new LocalizerFormattedString(this, args);

    protected override string LocalizeInternal(IStringLocalizerFactory localizerFactory, object[] arguments)
    {
        if (resourceType is null)
        {
            return ToString();
        }

        var localizer = localizerFactory.Create(resourceType);
        if (arguments.Length == 0)
        {
            return localizer[format.Name];
        }

        return localizer[format.Name, arguments];
    }
}

public class LazyFormattedString : FormattedString
{
    private readonly Func<string> getter;

    public LazyFormattedString(Func<string> getter) : base(getter())
    {
        this.getter = getter;
    }

    private LazyFormattedString(LazyFormattedString copy, object[] args) : base(copy, args)
    {
        getter = copy.getter;
    }

    public override FormattedString WithArgs(params object[] args)
        => new LazyFormattedString(this, args);

    protected override string LocalizeInternal(IStringLocalizerFactory localizerFactory, object[] arguments)
        => string.Format(getter(), arguments);
}
