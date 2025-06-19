using System.Runtime.Serialization;
using Microsoft.Extensions.Localization;
using Saritasa.Tools.Domain.Localization;

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
    public LocalizableException(FormattedString message) :
        base(message.ToString())
    {
        formattedString = message;
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
    public LocalizableException(FormattedString message, Exception innerException) :
        base(message.ToString(), innerException)
    {
        formattedString = message;
    }

    /// <summary>
    /// Get localized message.
    /// </summary>
    /// <param name="localizerFactory">Localizer factory.</param>
    public virtual string GetLocalizedMessage(IStringLocalizerFactory localizerFactory)
    {
        if (formattedString == null)
        {
            return base.Message;
        }

        return localizerFactory.Format(formattedString);
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
