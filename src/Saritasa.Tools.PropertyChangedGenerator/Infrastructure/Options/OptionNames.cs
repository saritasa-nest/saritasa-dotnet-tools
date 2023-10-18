using System.ComponentModel;

namespace Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;

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
    /// Represents a naming convention of generated backing fields.
    /// </summary>
    public const string BackingFieldsNamingConventionName = "property_changed_backing_fields_convention";

    /// <summary>
    /// Represents a value which indicates whether backing field should use underscore at beginning of its name.
    /// </summary>
    public const string BackingFieldsUseUnderscoreName = "property_changed_backing_fields_underscore";

    /// <summary>
    /// Represents an array of method names that will be picked by application to raise <see cref="INotifyPropertyChanged"/> event.
    /// </summary>
    public const string PropertyChangedMethodNames = "property_changed_raise_method_names";

    /// <summary>
    /// Represents an array of method names that will be picked by application to raise <see cref="INotifyPropertyChanging"/> event.
    /// </summary>
    public const string PropertyChangingMethodNames = "property_changing_raise_method_names";
}
