using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Saritasa.Tools.Domain.Localization;
using Xunit;

namespace Saritasa.Tools.Domain.Tests;

public class LocalizationTests
{
    private readonly IStringLocalizerFactory stringLocalizerFactory;
    private readonly IStringLocalizer<Strings> stringLocalizer;

    public LocalizationTests()
    {
        stringLocalizerFactory = new ResourceManagerStringLocalizerFactory(
            Options.Create(new LocalizationOptions()),
            NullLoggerFactory.Instance);

        stringLocalizer = new StringLocalizer<Strings>(stringLocalizerFactory);
    }

    [Fact]
    public void FormattedString_ToString_IsCultureIndependent()
    {
        var expirationDate = new DateTime(2025, 7, 3);
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var localizedMessage = stringLocalizer[nameof(Strings.LicenseExpired_Error), expirationDate];
        var localizerFormattedMessage = stringLocalizer.CreateFromResource(Strings.LicenseExpired_Error).WithArgs(expirationDate);
        var typeFormattedMessage = FormattedString.FromResource<Strings>(Strings.LicenseExpired_Error).WithArgs(expirationDate);
        var lazyFormattedMessage = FormattedString.FromLambda(() => Strings.LicenseExpired_Error).WithArgs(expirationDate);

        Assert.Equal("The license expired on 07/03/2025.", localizedMessage);
        Assert.Equal("The license expired on 07/03/2025.", localizerFormattedMessage.ToString());
        Assert.Equal("The license expired on 07/03/2025.", typeFormattedMessage.ToString());
        Assert.Equal("The license expired on 07/03/2025.", lazyFormattedMessage.ToString());

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("The license expired on 07/03/2025.", localizedMessage);
        Assert.Equal("The license expired on 07/03/2025.", localizerFormattedMessage.ToString());
        Assert.Equal("The license expired on 07/03/2025.", typeFormattedMessage.ToString());
        Assert.Equal("The license expired on 07/03/2025.", lazyFormattedMessage.ToString());
    }

    [Fact]
    public void FormattedString_Localize_IsCultureDependent()
    {
        var expirationDate = new DateTime(2025, 7, 3);
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var localizerFormattedMessage = stringLocalizer.CreateFromResource(Strings.LicenseExpired_Error).WithArgs(expirationDate);
        var typeFormattedMessage = FormattedString.FromResource<Strings>(Strings.LicenseExpired_Error).WithArgs(expirationDate);
        var lazyFormattedMessage = FormattedString.FromLambda(() => Strings.LicenseExpired_Error).WithArgs(expirationDate);

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("Срок лицензии истек 03.07.2025.", typeFormattedMessage.Localize(stringLocalizerFactory));
        Assert.Equal("Срок лицензии истек 03.07.2025.", lazyFormattedMessage.Localize(stringLocalizerFactory));
        Assert.Equal("Срок лицензии истек 03.07.2025.", localizerFormattedMessage.Localize(stringLocalizerFactory));
    }
}
