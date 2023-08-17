using System.ComponentModel;

namespace Saritasa.Tools.SourceGenerator.Models.Analyzers;

/// <summary>
/// Field analysis.
/// </summary>
public record FieldAnalysis : MemberAnalysis
{
    /// <summary>
    /// Should ignore <see cref="INotifyPropertyChanged.PropertyChanged"/> firing.
    /// </summary>
    public bool DoNotNotify { get; set; }

    /// <summary>
    /// Also notify members.
    /// </summary>
    public IEnumerable<string> AlsoNotifyMembers { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Associated field property.
    /// </summary>
    public PropertyAnalysis? AssociatedProperty { get; set; }
}
