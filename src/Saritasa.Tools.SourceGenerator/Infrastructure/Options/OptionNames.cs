using System.ComponentModel;

namespace Saritasa.Tools.SourceGenerator.Infrastructure.Options;

/// <summary>
/// Represents an application option names.
/// </summary>
internal static class OptionNames
{
    /// <summary>
    /// Represents an user indent style.
    /// </summary>
    public const string IndentStyleName = "indent_style";

    /// <summary>
    /// Represents an user indent size.
    /// </summary>
    public const string IndentSizeName = "indent_size";

    /// <summary>
    /// Represents an array of method names that will be picked by application to raise <see cref="INotifyPropertyChanged"/> event.
    /// </summary>
    public const string PropertyChangedMethodNames = "property_changed_raise_method_names";

    /// <summary>
    /// Represents an array of method names that will be picked by application to raise <see cref="INotifyPropertyChanging"/> event.
    /// </summary>
    public const string PropertyChangingMethodNames = "property_changing_raise_method_names";
}
