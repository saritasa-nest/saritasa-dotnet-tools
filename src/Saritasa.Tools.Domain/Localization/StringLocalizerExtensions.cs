using System.Runtime.CompilerServices;
using Saritasa.Tools.Domain.Localization;

namespace Microsoft.Extensions.Localization;

/// <summary>
/// Extension methods for <see cref="IStringLocalizer"/>.
/// See also <seealso href="https://github.com/saritasa-nest/saritasa-dotnet-tools/wiki/Domain-Localization"/>.
/// </summary>
public static class StringLocalizerExtensions
{
    /// <summary>
    /// Create formatted string using <paramref name="localizer"/> resource type, resource key and arguments.
    /// For example usage see <seealso cref="Format"/>.
    /// </summary>
    /// <typeparam name="T">Resource provider class.</typeparam>
    /// <param name="localizer">Localizer.</param>
    /// <param name="resourceName">Resource name.</param>
    public static FormattedString GetFormatted<T>(
        this IStringLocalizer<T> localizer,
        string resourceName,
        params object[] args)
    {
        return new LocalizedFormattedString<T>(localizer[resourceName!], args);
    }

    /// <summary>
    /// Format message using localization resources.
    /// <para>
    /// Unlike <see cref="IStringLocalizer.get_Item(string, object[])"/>, this extension method
    /// applies recursive localization to <see cref="FormattedString.Arguments"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Say, you have a parameterized localized string:
    /// <code>
    /// public static partial class Strings
    /// {
    ///     public static string LicenseExpired_Error => GetResource(
    ///         nameof(LicenseExpired_Error),
    ///         "The license expired on {0:d}.");
    /// }
    /// </code>
    ///
    /// <list type="number">
    /// <item>
    /// The following code:
    /// <code>
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("en-US");
    /// localizer[nameof(Strings.LicenseExpired_Error), licenseExpiredAt];
    /// </code>
    /// Will produce "The license expired on 7/3/2025".
    /// </item>
    /// <item>
    /// The following code:
    /// <code>
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("ru-RU");
    /// localizer[nameof(Strings.LicenseExpired_Error), licenseExpiredAt];
    /// </code>
    /// Will produce "Срок лицензии истек 03.07.2025.".
    /// </item>
    ///
    /// <item>
    /// The following code:
    /// <code>
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("ru-RU");
    /// localizer[Strings.LicenseExpired_Error, licenseExpiredAt];
    /// </code>
    /// Will fail because it will use resource string value as a key.
    /// </item>
    /// </list>
    /// <para />
    ///
    /// Wrapping strings to <see cref="CreateFromResource{T}(IStringLocalizer{T}, string, string?)" />
    /// will produce the localizable <see cref="FormattedString"/>.
    /// <code>
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("en-US");
    /// var formatted = localizer
    ///     .CreateFromResource(Strings.LicenseExpired_Error)
    ///     .WithArgs(part);
    ///
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("ru-RU");
    /// localizer.Format(formatted);
    /// </code>
    /// Will also produce "Срок лицензии истек 03.07.2025.".
    /// </remarks>
    /// <param name="localizerFactory">Localizer factory.</param>
    /// <param name="formattedString">Formatted string.</param>
    public static string Format(
        this IStringLocalizerFactory localizerFactory,
        FormattedString formattedString)
    {
        return formattedString.Localize(localizerFactory);
    }
}
