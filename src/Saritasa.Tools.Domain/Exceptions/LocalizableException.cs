using System.Runtime.Serialization;
using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Exceptions;

/// <summary>
/// Localizable exception.
/// </summary>
public class LocalizableException : Exception
{
    private readonly FormattedString? formattedString;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LocalizableException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public LocalizableException(string message, Exception innerException) :
        base(message, innerException)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    /// <param name="args">Localized string arguments.</param>
    public LocalizableException(Exception innerException, LocalizedString message, params object[] args) :
        base(args?.Length > 0 ? string.Format(message, args) : message, innerException)
    {
        formattedString = new(message, args);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="args">Localized string arguments.</param>
    public LocalizableException(LocalizedString message, params object[] args) :
        base(args?.Length > 0 ? string.Format(message, args) : message)
    {
        formattedString = new(message, args);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LocalizableException(LocalizedString message) :
        base(message)
    {
        formattedString = new(message);
    }

    /// <summary>
    /// Get localized message.
    /// </summary>
    /// <param name="localizer">Localizer.</param>
    public virtual string GetLocalizedMessage(IStringLocalizer localizer)
    {
        if (formattedString == null)
        {
            return base.Message;
        }

        return localizer.Format(formattedString.Value);
    }

    /// <summary>
    /// Constructor for deserialization.
    /// </summary>
    /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
    /// <param name="context">Describes the source and destination of a given serialized stream,
    /// and provides an additional caller-defined context.</param>
    protected LocalizableException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
