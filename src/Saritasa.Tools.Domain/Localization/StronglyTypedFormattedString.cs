using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Formatted string implementation that uses lambda function
/// to a strongly typed resource provider and resolves localization ad-hoc.
/// </summary>
internal class StronglyTypedFormattedString : FormattedString
{
    private readonly Func<string> stronglyTypedResourceLookup;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="stronglyTypedResourceLookup">Resource lookup resolver.</param>
    /// <param name="args">Formattable arguments.</param>
    public StronglyTypedFormattedString(Func<string> stronglyTypedResourceLookup, object[] args)
        : base(stronglyTypedResourceLookup(), args)
    {
        this.stronglyTypedResourceLookup = stronglyTypedResourceLookup;
    }

    /// <summary>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    /// <param name="localizerFactory">This implementation does not use <see cref="IStringLocalizer"/> to get localized values.</param>
    /// <param name="localizedArguments"><inheritdoc path="/param[@name='localizedArguments']"/></param>
    protected override string LocalizeInternal(
        IStringLocalizerFactory localizerFactory,
        object[] localizedArguments) => string.Format(stronglyTypedResourceLookup(), localizedArguments);
}
