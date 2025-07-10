using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Generic class for formatted string.
/// Reuses the same ResourceType object per the closed generic class.
/// </summary>
internal class LocalizedFormattedString<TResource> : FormattedString
{
    private static readonly Type resourceType = typeof(TResource);

    private readonly LocalizedString format;

    /// <param name="format">Formatting pattern as localized string. Contains the name of the resource and the neutral value.</param>
    public LocalizedFormattedString(LocalizedString format, object[] args)
        : base(format.Value, args)
    {
        this.format = format;
    }

    protected override string LocalizeInternal(
        IStringLocalizerFactory localizerFactory,
        object[] arguments)
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
