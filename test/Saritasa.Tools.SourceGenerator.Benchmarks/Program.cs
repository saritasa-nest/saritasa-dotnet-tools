using System.ComponentModel;

namespace Saritasa.Tools.SourceGenerator.Benchmark;

/// <summary>
/// Editable model.
/// </summary>
public abstract class EditableModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    private string hiddenField;

    /// <summary>
    /// Ignored public mock field.
    /// </summary>
    [DoNotNotify]
    public string baseIgnoredField;

    protected string protectedField;

    /// <summary>
    /// Public mock field.
    /// </summary>
    [AlsoNotify(nameof(baseReflectedField))]
    public string publicField;

    public string baseReflectedField;
}

/// <summary>
/// Mock view model.
/// </summary>
public partial class ViewModel : EditableModel
{
    private int implementedField;

    /// <summary>
    /// Implemented mock field.
    /// </summary>
    public int ImplementedField
    {
        get => implementedField;
        set => implementedField = value;
    }

    [AlsoNotify(nameof(reflectedField))]
    private string field;

    private string reflectedField;

    [DoNotNotify]
    private string ignoreField;

    private int? nullableField;
}

/// <summary>
/// Application point of entry.
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("App started.");

        try
        {
            var vm = new ViewModel();

            vm.PropertyChanging += Vm_PropertyChanging;
            vm.PropertyChanged += Vm_PropertyChanged;
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private static void Vm_PropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        // Property changing.
    }

    private static void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Property changed.
    }
}
