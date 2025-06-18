using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Exceptions;

/// <summary>
/// Extension methods for <see cref="IStringLocalizer"/>.
/// </summary>
public static class StringLocalizerExtensions
{
    /// <summary>
    /// Format message using localization resources.
    /// </summary>
    /// <param name="localizer"></param>
    /// <param name="formattedString"></param>
    /// <returns></returns>
    public static LocalizedString Format(this IStringLocalizer localizer, FormattedString formattedString)
    {
        if (formattedString.Arguments is null)
        {
            return localizer[formattedString.Format.Name];
        }

        return localizer[formattedString.Format.Name, formattedString.Arguments];
    }
}
