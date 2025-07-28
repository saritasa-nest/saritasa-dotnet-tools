using System.Globalization;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Domain.Localization;
using Saritasa.Tools.Domain.Tests.Resources;
using Xunit;

namespace Saritasa.Tools.Domain.Tests;

/// <summary>
/// Tests for <see cref="StronglyTypedFormattedString"/>.
/// </summary>
public class StronglyTypedLocalizationTests
{
    private readonly DateTime expirationDate;

    /// <summary>
    /// Constructor.
    /// </summary>
    public StronglyTypedLocalizationTests()
    {
        expirationDate = new DateTime(2025, 7, 3);
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Check that <see cref="StronglyTypedFormattedString"/> persists its internal state
    /// after switching the <see cref="CultureInfo.CurrentUICulture"/>. The <see cref="FormattedString.ToString"/> is invariant.
    /// </summary>
    [Fact]
    public void FormattedString_ToString_IsCultureIndependent()
    {
        var message = FormattedString.FromResource(() =>
            TypedStrings.LicenseExpired_Error_1, expirationDate);

        Assert.Equal("The license expired on 07/03/2025.", message.ToString());

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("The license expired on 07/03/2025.", message.ToString());
    }

    /// <summary>
    /// Check that <see cref="StronglyTypedFormattedString"/> varies its localization behaviour
    /// after switching the <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    [Fact]
    public void FormattedString_Localize_IsCultureDependent()
    {
        var message = FormattedString.FromResource(() =>
            TypedStrings.LicenseExpired_Error_1, expirationDate);

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("Срок лицензии истек 03.07.2025.", message.Localize(stringLocalizerFactory: default!));
    }

    /// <summary>
    /// Check that <see cref="StronglyTypedFormattedString"/> constructor can accept args or have them captured in lambda.
    /// </summary>
    [Fact]
    public void FormattedString_Format_IsComposed()
    {
        var postFormattedMessage = FormattedString.FromResource(() =>
            TypedStrings.LicenseExpired_Error_1, expirationDate);

        var preFormattedMessage = FormattedString.FromResource(() =>
            string.Format(TypedStrings.LicenseExpired_Error_1, expirationDate));

        Assert.Equal("The license expired on 07/03/2025.", preFormattedMessage.ToString());
        Assert.Equal("The license expired on 07/03/2025.", postFormattedMessage.ToString());

        Assert.Equal("The license expired on 07/03/2025.", preFormattedMessage.Localize(stringLocalizerFactory: default!));
        Assert.Equal("The license expired on 07/03/2025.", postFormattedMessage.Localize(stringLocalizerFactory: default!));

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("The license expired on 07/03/2025.", preFormattedMessage.ToString());
        Assert.Equal("The license expired on 07/03/2025.", postFormattedMessage.ToString());

        Assert.Equal("Срок лицензии истек 03.07.2025.", preFormattedMessage.Localize(stringLocalizerFactory: default!));
        Assert.Equal("Срок лицензии истек 03.07.2025.", postFormattedMessage.Localize(stringLocalizerFactory: default!));
    }

    /// <summary>
    /// Check that <see cref="DomainException"/> does not break the state of internal <see cref="FormattedString"/>.
    /// Also, that <see cref="Exception.Message"/> is invariant, and culture independent.
    /// </summary>
    [Fact]
    public void LocalizedException_Message_IsCultureDependent()
    {
        try
        {
            throw new DomainException(FormattedString.FromResource(() =>
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

    /// <summary>
    /// Check that <see cref="ValidationException"/> does not break the state of internal <see cref="FormattedString"/>.
    /// Also, that <see cref="Exception.Message"/> is invariant, and culture independent.
    /// </summary>
    [Fact]
    public void ValidationException_Message_IsCultureDependent()
    {
        try
        {
            throw new ValidationException(ValidationErrors.CreateFromErrors(
                nameof(expirationDate),
                FormattedString.FromResource(() => TypedStrings.LicenseExpired_Error_1, expirationDate)));
        }
        catch (ValidationException ex)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            Assert.Equal(
                "Validation errors.",
                ex.Message);

            Assert.Equal(
                "Ошибки при проверке данных.",
                ex.GetLocalizedMessage(localizer: default!));

            ValidationException.MessageFormatter = ValidationErrorsFormatter.GroupErrorsOrDefaultMessageFormatter;

            Assert.Equal(
                "- expirationDate: The license expired on 07/03/2025.\r\n",
                ex.Message);

            Assert.Equal(
                "- expirationDate: Срок лицензии истек 03.07.2025.\r\n",
                ex.GetLocalizedMessage(localizer: default!));
        }
    }
}
