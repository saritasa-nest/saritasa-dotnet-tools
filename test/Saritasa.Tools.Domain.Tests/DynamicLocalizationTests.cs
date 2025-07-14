using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Saritasa.Tools.Domain.Localization;
using Xunit;

namespace Saritasa.Tools.Domain.Tests;

/// <summary>
/// Tests for <see cref="LocalizedFormattedString{TResource}"/>.
/// </summary>
public class DynamicLocalizationTests
{
    private readonly DateTime expirationDate;
    private readonly IStringLocalizerFactory stringLocalizerFactory;
    private readonly IStringLocalizer<DynamicLocalizationTests> stringLocalizer;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DynamicLocalizationTests()
    {
        expirationDate = new DateTime(2025, 7, 3);
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        stringLocalizerFactory = new ResourceManagerStringLocalizerFactory(
            Options.Create(new LocalizationOptions()),
            NullLoggerFactory.Instance);

        stringLocalizer = new StringLocalizer<DynamicLocalizationTests>(stringLocalizerFactory);
    }

    /// <summary>
    /// Check that <see cref="LocalizedFormattedString{TResource}"/> works even if resource key is not found in an associated resource file.
    /// After switching the <see cref="CultureInfo.CurrentUICulture"/>, the <see cref="FormattedString.ToString"/> equals to <see cref="LocalizedString.Name"/>.
    /// </summary>
    [Fact]
    public void FormattedString_ResourceKey_IsNotRequired()
    {
        var localizedMessage = stringLocalizer["This string is not in DynamicLocalizationTests.ru.resx."];
        var localizerFormattedMessage = stringLocalizer.GetFormatted("This string is not in DynamicLocalizationTests.ru.resx.");

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("This string is not in DynamicLocalizationTests.ru.resx.", localizedMessage);
        Assert.Equal("This string is not in DynamicLocalizationTests.ru.resx.", localizerFormattedMessage.Localize(stringLocalizerFactory));
    }

    /// <summary>
    /// Check that <see cref="LocalizedFormattedString{TResource}"/> persists its internal state
    /// after switching the <see cref="CultureInfo.CurrentUICulture"/>. The <see cref="FormattedString.ToString"/> is invariant.
    /// </summary>
    [Fact]
    public void FormattedString_ToString_IsCultureIndependent()
    {
        var localizedMessage = stringLocalizer["The license expired on {0:d}.", expirationDate];
        var localizerFormattedMessage = stringLocalizer.GetFormatted("The license expired on {0:d}.", expirationDate);

        Assert.Equal("The license expired on 07/03/2025.", localizedMessage);
        Assert.Equal("The license expired on 07/03/2025.", localizerFormattedMessage.ToString());

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("The license expired on 07/03/2025.", localizedMessage);
        Assert.Equal("The license expired on 07/03/2025.", localizerFormattedMessage.ToString());
    }

    /// <summary>
    /// Check that <see cref="LocalizedFormattedString{TResource}"/> varies its localization behaviour
    /// after switching the <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    [Fact]
    public void FormattedString_Localize_IsCultureDependent()
    {
        var localizedMessage = stringLocalizer["The license expired on {0:d}.", expirationDate];
        var localizerFormattedMessage = stringLocalizer.GetFormatted("The license expired on {0:d}.", expirationDate);

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("The license expired on 07/03/2025.", localizedMessage);
        Assert.Equal("Срок лицензии истек 03.07.2025.", localizerFormattedMessage.Localize(stringLocalizerFactory));
    }
}
