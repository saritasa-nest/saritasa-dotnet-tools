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

    /// <inheritdoc />
    protected override string LocalizeInternal(
        IStringLocalizerFactory localizerFactory,
        object[] arguments) => string.Format(stronglyTypedResourceLookup(), arguments);
}
