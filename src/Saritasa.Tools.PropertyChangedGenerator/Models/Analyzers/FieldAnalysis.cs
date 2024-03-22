using System.ComponentModel;
using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;

/// <summary>
/// Field analysis.
/// </summary>
public record FieldAnalysis : MemberAnalysis
{
    private readonly List<AttributeAnalysis> attributes = new();

    /// <summary>
    /// Should ignore <see cref="INotifyPropertyChanged.PropertyChanged"/> firing.
    /// </summary>
    public bool DoNotNotify { get; set; }

    /// <summary>
    /// Getter accessibility.
    /// </summary>
    public Accessibility? GetterAccessibility { get; set; }

    /// <summary>
    /// Setter accessibility.
    /// </summary>
    public Accessibility? SetterAccessibility { get; set; }

    /// <summary>
    /// Also notify members.
    /// </summary>
    public IEnumerable<string> AlsoNotifyMembers { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Field attributes.
    /// </summary>
    public IEnumerable<AttributeAnalysis> Attributes => attributes;

    /// <summary>
    /// Associated field property.
    /// </summary>
    public PropertyAnalysis? AssociatedProperty { get; set; }

    /// <summary>
    /// Add an attribute analysis.
    /// </summary>
    /// <param name="analysis">Attribute analysis.</param>
    public void AddAttribute(AttributeAnalysis analysis)
        => attributes.Add(analysis);
}
