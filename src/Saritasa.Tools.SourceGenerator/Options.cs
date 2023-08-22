using System.ComponentModel;

namespace Saritasa.Tools.SourceGenerator;

/// <summary>
/// Represents an application option keys.
/// </summary>
internal static class Options
{
    /// <summary>
    /// Represents an array of method names that will be picked by application to raise <see cref="INotifyPropertyChanged"/> event.
    /// </summary>
    public const string PropertyChangedMethodNames = "property_changed_raise_method_names";

    /// <summary>
    /// Represents an array of method names that will be picked by application to raise <see cref="INotifyPropertyChanging"/> event.
    /// </summary>
    public const string PropertyChangingMethodNames = "property_changing_raise_method_names";
}
