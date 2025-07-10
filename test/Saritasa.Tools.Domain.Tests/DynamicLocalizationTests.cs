using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Saritasa.Tools.Domain.Tests;

public class DynamicLocalizationTests
{
    private readonly DateTime expirationDate;
    private readonly IStringLocalizerFactory stringLocalizerFactory;
    private readonly IStringLocalizer<DynamicLocalizationTests> stringLocalizer;

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
