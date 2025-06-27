using Saritasa.Tools.Domain.Localization;

namespace Microsoft.Extensions.Localization;

/// <summary>
/// Extension methods for <see cref="IStringLocalizer"/>.
/// </summary>
public static class StringLocalizerExtensions
{
    /// <summary>
    /// Create formatted string using <paramref name="localizer"/> resource type, resource key and arguments.
    /// For example usage see <seealso cref="Format"/>.
    /// </summary>
    /// <typeparam name="T">Resource provider class.</typeparam>
    /// <param name="localizer">Localizer.</param>
    /// <param name="nameOrFormat">Resource name.</param>
    /// <param name="args">Formattable arguments.</param>
    public static FormattedString Create<T>(this IStringLocalizer<T> localizer, string nameOrFormat, params object[] args)
    {
        return new FormattedString<T>(localizer[nameOrFormat], args);
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
    /// var genericMessage = new LocalizedString(
    ///     name: "ValidationError_Generic",
    ///     value: "Validation error ocurred: '{0}'");
    ///
    /// var concreteMessage = new LocalizedString(
    ///     name: "ValidationError_RequiredEmail",
    ///     value: "Email is required.");
    /// </code>
    ///
    /// <list type="number">
    /// <item>
    /// The following code:
    /// <code>
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("en-US");
    /// localizer[genericMessage.Name, concreteMessage];
    /// </code>
    /// Will result in "Validation error ocurred: 'Email is required.'".
    /// </item>
    /// <item>
    /// The following code:
    /// <code>
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("ru-RU");
    /// localizer[genericMessage.Name, concreteMessage];
    /// </code>
    /// Will result in "Ошибка валидации: 'Email is required.'".
    /// </item>
    /// </list>
    /// <para />
    ///
    /// Wrapping strings to <see cref="Create{T}(IStringLocalizer{T}, string, object[])" />
    /// will produce the localizable <see cref="FormattedString"/>.
    /// <code>
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("en-US");
    /// var part = localizer.Create(concreteMessage.Name);
    /// var formatted = localizer.Create(genericMessage.Name, part);
    ///
    /// CultureInfo.CurrentUICulture = CultureInfo.GetCulture("ru-RU");
    /// localizer.Format(formatted);
    /// </code>
    /// Will result in "Ошибка валидации: 'Требуется E-mail.'".
    /// </remarks>
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

        var arguments = formattedString.Arguments.Select(arg => arg switch
        {
            FormattedString fs => localizerFactory.Format(fs),
            var obj => obj,
        });

        return localizer[name, arguments];
    }
}
