using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Formatted string implementation that uses <see cref="LocalizedString"/> for resource identification.
/// </summary>
/// <typeparam name="TResource">Resource provider class associated with a resx file.</typeparam>
internal class LocalizedFormattedString<TResource> : FormattedString
{
    private static readonly Type resourceType = typeof(TResource);

    private readonly LocalizedString format;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="format">Formatting pattern as localized string. Contains the name of the resource and the neutral value.</param>
    /// <param name="args">Formattable arguments.</param>
    public LocalizedFormattedString(LocalizedString format, object[] args)
        : base(format.Value, args)
    {
        this.format = format;
    }

    /// <inheritdoc />
    protected override string LocalizeInternal(
        IStringLocalizerFactory localizerFactory,
        object[] localizedArguments)
    {
        if (resourceType is null)
        {
            return base.LocalizeInternal(localizerFactory, localizedArguments);
        }

        var localizer = localizerFactory.Create(resourceType);
        if (localizedArguments.Length == 0)
        {
            return localizer[format.Name];
        }

        return localizer[format.Name, localizedArguments];
    }
}
