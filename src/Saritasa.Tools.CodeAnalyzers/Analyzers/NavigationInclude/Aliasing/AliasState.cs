using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// The picture of one position: which objects each slot holds. Variables are slots of
/// <see cref="AliasObject.Frame"/>, properties are slots of the objects they belong to.
/// </summary>
/// <remarks>
/// A reference such as <c>user.Profile.Settings</c> is not stored. It is resolved by starting at the frame and
/// following one slot per name, so an assignment changes one entry, and every alias sees the change.
/// The state is immutable: every operation returns a new one.
/// </remarks>
internal sealed class AliasState : IEquatable<AliasState>
{
    private readonly ImmutableDictionary<Slot, ObjectSet> slots;

    // Properties assigned through a receiver nothing is known about. Such an assignment may have changed the
    // property on any object, including slots that were never stored.
    private readonly ImmutableHashSet<object> propertiesAssignedOnUnknown;

    private AliasState(
        ImmutableDictionary<Slot, ObjectSet> slots,
        ImmutableHashSet<object> propertiesAssignedOnUnknown)
    {
        this.slots = slots;
        this.propertiesAssignedOnUnknown = propertiesAssignedOnUnknown;
    }

    /// <summary>
    /// The picture before anything is assigned.
    /// </summary>
    public static AliasState Empty { get; } = new(
        ImmutableDictionary<Slot, ObjectSet>.Empty,
        ImmutableHashSet.Create<object>(VariableComparer.Instance));

    /// <summary>
    /// Returns the objects a slot holds. A slot nothing assigned holds the object it had when the method started.
    /// </summary>
    /// <remarks>
    /// For a parameter that is the object the caller passed in. A local is never read before it is assigned, since
    /// C# requires it.
    /// </remarks>
    /// <param name="slot">Slot.</param>
    /// <returns>Objects.</returns>
    public ObjectSet Read(Slot slot)
    {
        if (slots.TryGetValue(slot, out var objects))
        {
            return objects;
        }

        // "node = node.Next" in a loop would name a deeper initial object on every pass, and the pass would never
        // stop. Past the limit the object is not named.
        if (slot.Owner.Depth >= AliasObject.MaxDepth)
        {
            return ObjectSet.Unknown;
        }

        var initial = ObjectSet.Of(AliasObject.InitialValueOf(slot));

        return propertiesAssignedOnUnknown.Contains(slot.Member)
            ? initial.Union(ObjectSet.Unknown)
            : initial;
    }

    /// <summary>
    /// Resolves a reference: the variable, then one property per step, e.g. <c>user</c>, Profile, Settings for
    /// <c>user.Profile.Settings</c>.
    /// </summary>
    /// <param name="variable">Variable the reference starts at.</param>
    /// <param name="properties">Properties after the variable, in order.</param>
    /// <returns>Objects the reference may hold.</returns>
    public ObjectSet Resolve(object variable, IEnumerable<IPropertySymbol> properties)
    {
        var objects = Read(Slot.OfVariable(variable));

        foreach (var property in properties)
        {
            objects = ReadProperty(objects, property);
        }

        return objects;
    }

    /// <summary>
    /// Returns the objects <c>owner.Property</c> may hold, for every object the owner may be.
    /// </summary>
    /// <param name="owners">Objects of the expression before the property.</param>
    /// <param name="property">Property.</param>
    /// <returns>Objects.</returns>
    public ObjectSet ReadProperty(ObjectSet owners, IPropertySymbol property)
    {
        if (owners.IsUnknown)
        {
            return ObjectSet.Unknown;
        }

        var objects = ObjectSet.Empty;

        foreach (var owner in owners.Objects)
        {
            objects = objects.Union(Read(new Slot(owner, property)));
        }

        return objects;
    }

    /// <summary>
    /// Returns the picture after <c>variable = value</c>.
    /// </summary>
    /// <param name="variable">Assigned variable.</param>
    /// <param name="value">Objects of the assigned value.</param>
    /// <returns>New picture.</returns>
    public AliasState AssignVariable(object variable, ObjectSet value)
        => Assign(ObjectSet.Of(AliasObject.Frame), variable, value);

    /// <summary>
    /// Returns the picture after <c>receiver.Property = value</c>.
    /// </summary>
    /// <param name="receiver">Objects of the expression before the property.</param>
    /// <param name="property">Assigned property.</param>
    /// <param name="value">Objects of the assigned value.</param>
    /// <returns>New picture.</returns>
    public AliasState AssignProperty(ObjectSet receiver, IPropertySymbol property, ObjectSet value)
        => Assign(receiver, property, value);

    /// <summary>
    /// Returns the picture where two branches meet: each slot holds what it holds in either one.
    /// </summary>
    /// <param name="other">Picture of the other branch.</param>
    /// <returns>Combined picture.</returns>
    public AliasState Combine(AliasState other)
    {
        // A slot one branch has no entry for still holds its initial object there, which Read returns.
        var combinedSlots = slots.Keys
            .Concat(other.slots.Keys)
            .Distinct()
            .Select(slot => new KeyValuePair<Slot, ObjectSet>(slot, Read(slot).Union(other.Read(slot))));

        return new(
            ImmutableDictionary<Slot, ObjectSet>.Empty.SetItems(combinedSlots),
            propertiesAssignedOnUnknown.Union(other.propertiesAssignedOnUnknown));
    }

    /// <summary>
    /// True if both pictures hold the same objects everywhere. The forward pass stops repeating once no block
    /// start changes.
    /// </summary>
    /// <param name="other">Other picture.</param>
    /// <returns>True if equal.</returns>
    public bool Equals(AliasState? other)
        => other is not null &&
           slots.Count == other.slots.Count &&
           slots.All(entry => other.slots.TryGetValue(entry.Key, out var objects) && objects.Equals(entry.Value)) &&
           propertiesAssignedOnUnknown.SetEquals(other.propertiesAssignedOnUnknown);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AliasState);

    /// <inheritdoc />
    public override int GetHashCode()
        => unchecked((slots.Count * 31) + propertiesAssignedOnUnknown.Count);

    /// <summary>
    /// Returns the picture after <c>owner.Member = value</c>. A sure owner has its slot replaced. Several owners each
    /// may or may not have been assigned, so the value is added to what their slots held.
    /// </summary>
    private AliasState Assign(ObjectSet owners, object member, ObjectSet value)
    {
        if (owners.IsUnknown)
        {
            // Any object may be the owner, so every slot with this member may now hold the value.
            var changedSlots = slots
                .Where(entry => VariableComparer.Instance.Equals(entry.Key.Member, member))
                .Select(entry => new KeyValuePair<Slot, ObjectSet>(
                    entry.Key,
                    entry.Value.Union(value).Union(ObjectSet.Unknown)));

            return new(slots.SetItems(changedSlots), propertiesAssignedOnUnknown.Add(member));
        }

        if (owners.IsSure)
        {
            var assignedSlot = new Slot(owners.Objects.First(), member);

            return new(slots.SetItem(assignedSlot, value), propertiesAssignedOnUnknown);
        }

        var addedSlots = owners.Objects
            .Select(owner => new Slot(owner, member))
            .Select(slot => new KeyValuePair<Slot, ObjectSet>(slot, Read(slot).Union(value)));

        return new(slots.SetItems(addedSlots), propertiesAssignedOnUnknown);
    }
}
