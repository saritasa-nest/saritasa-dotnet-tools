using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Extension methods for <see cref="IStringLocalizer"/>.
/// </summary>
public static class StringLocalizerExtensions
{
    public static FormattedString Create<T>(IStringLocalizer<T> localizer, string nameOrFormat, params object[] args)
    {
        return new FormattedString<T>(localizer[nameOrFormat], args);
    }

    /// <summary>
    /// Format message using localization resources.
    /// </summary>
    /// <param name="localizerFactory">Localizer factory.</param>
    /// <param name="formattedString">Formatted string.</param>
    public static LocalizedString Format(this IStringLocalizerFactory localizerFactory, FormattedString formattedString)
    {
        var name = formattedString.Format.Name;
        if (formattedString.ResourceType is null)
        {
            return new LocalizedString(name, formattedString.ToString());
        }

        var localizer = localizerFactory.Create(formattedString.ResourceType);

        if (formattedString.Arguments is null)
        {
            return localizer[name];
        }

        return localizer[name, formattedString.Arguments];
    }
}
