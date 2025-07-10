using System.Globalization;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Domain.Localization;
using Saritasa.Tools.Domain.Tests.Resources;
using Xunit;

namespace Saritasa.Tools.Domain.Tests;

public class StronglyTypedLocalizationTests
{
    private readonly DateTime expirationDate;

    public StronglyTypedLocalizationTests()
    {
        expirationDate = new DateTime(2025, 7, 3);
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void FormattedString_ToString_IsCultureIndependent()
    {
        var message = FormattedString.StronglyTypedResource(() =>
            TypedStrings.LicenseExpired_Error_1, expirationDate);

        Assert.Equal("The license expired on 07/03/2025.", message.ToString());

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("The license expired on 07/03/2025.", message.ToString());
    }

    [Fact]
    public void FormattedString_Localize_IsCultureDependent()
    {
        var message = FormattedString.StronglyTypedResource(() =>
            TypedStrings.LicenseExpired_Error_1, expirationDate);

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("Срок лицензии истек 03.07.2025.", message.Localize(stringLocalizerFactory: default!));
    }

    [Fact]
    public void LocalizedException_Message_IsCultureDependent()
    {
        try
        {
            throw new DomainException(FormattedString.StronglyTypedResource(() =>
                TypedStrings.LicenseExpired_Error_1, expirationDate));
        }
        catch (LocalizableException ex)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            Assert.Equal("The license expired on 07/03/2025.", ex.Message);
            Assert.Equal("Срок лицензии истек 03.07.2025.", ex.GetLocalizedMessage(localizerFactory: default!));
        }
    }
}
