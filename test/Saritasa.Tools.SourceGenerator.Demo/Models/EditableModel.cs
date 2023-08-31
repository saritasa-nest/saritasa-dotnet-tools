using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Saritasa.Tools.SourceGenerator.Demo.Models;

/// <summary>
/// Editable model.
/// </summary>
public abstract class EditableModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raise <see cref="INotifyPropertyChanging.PropertyChanging"/> event.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    protected void RaisePropertyChanging([CallerMemberName] string? propertyName = null)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }
}
