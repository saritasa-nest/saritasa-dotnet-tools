using Microsoft.Extensions.Localization;

namespace Saritasa.Tools.Domain.Localization;

public class StronglyTypedFormattedString : FormattedString
{
    private readonly Func<string> stronglyTypedResourceLookup;

    public StronglyTypedFormattedString(
        Func<string> stronglyTypedResourceLookup,
        object[] args) : base(stronglyTypedResourceLookup(), args)
    {
        this.stronglyTypedResourceLookup = stronglyTypedResourceLookup;
    }

    protected override string LocalizeInternal(
        IStringLocalizerFactory localizerFactory,
        object[] arguments) => string.Format(stronglyTypedResourceLookup(), arguments);
}
