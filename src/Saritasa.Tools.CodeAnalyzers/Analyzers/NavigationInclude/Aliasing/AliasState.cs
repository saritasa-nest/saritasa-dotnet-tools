using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// The picture of one position: which objects each variable holds, and which objects each edge holds.
/// </summary>
/// <remarks>
/// A reference such as <c>user.Profile.Settings</c> is not stored. It is resolved by starting at the variable and
/// following one edge per property, so an assignment changes one entry, and every alias sees the change.
/// The state is immutable: every operation returns a new one.
/// </remarks>
internal sealed class AliasState : IEquatable<AliasState>
{
    private readonly ImmutableDictionary<object, ObjectSet> variables;

    private readonly ImmutableDictionary<Edge, ObjectSet> edges;

    // Properties assigned through a receiver nothing is known about. Such an assignment may have changed the
    // property on any object, including edges that were never stored.
    private readonly ImmutableHashSet<IPropertySymbol> propertiesAssignedOnUnknown;

    private AliasState(
        ImmutableDictionary<object, ObjectSet> variables,
        ImmutableDictionary<Edge, ObjectSet> edges,
        ImmutableHashSet<IPropertySymbol> propertiesAssignedOnUnknown)
    {
        this.variables = variables;
        this.edges = edges;
        this.propertiesAssignedOnUnknown = propertiesAssignedOnUnknown;
    }

    /// <summary>
    /// The picture before anything is assigned.
    /// </summary>
    public static AliasState Empty { get; } = new(
        ImmutableDictionary.Create<object, ObjectSet>(VariableComparer.Instance),
        ImmutableDictionary<Edge, ObjectSet>.Empty,
        ImmutableHashSet.Create<IPropertySymbol>(SymbolEqualityComparer.Default));

    /// <summary>
    /// Returns the objects a variable holds. A variable the picture has no entry for holds unknown objects.
    /// </summary>
    /// <param name="variable">Local, parameter or compiler temporary.</param>
    /// <returns>Objects.</returns>
    public ObjectSet ReadVariable(object variable)
        => variables.TryGetValue(variable, out var objects) ? objects : ObjectSet.Unknown;

    /// <summary>
    /// Returns the objects an edge holds. An edge nothing assigned holds the object it had when the method started.
    /// </summary>
    /// <param name="edge">Edge.</param>
    /// <returns>Objects.</returns>
    public ObjectSet ReadEdge(Edge edge)
    {
        if (edges.TryGetValue(edge, out var objects))
        {
            return objects;
        }

        var initial = ObjectSet.Of(AliasObject.InitialValueOf(edge));

        return propertiesAssignedOnUnknown.Contains(edge.Property)
            ? initial.Union(ObjectSet.Unknown)
            : initial;
    }

    /// <summary>
    /// Resolves a reference: the variable, then one edge per property, e.g. <c>user</c>, Profile, Settings for
    /// <c>user.Profile.Settings</c>.
    /// </summary>
    /// <param name="variable">Variable the reference starts at.</param>
    /// <param name="properties">Properties after the variable, in order.</param>
    /// <returns>Objects the reference may hold.</returns>
    public ObjectSet Resolve(object variable, IEnumerable<IPropertySymbol> properties)
    {
        var objects = ReadVariable(variable);

        foreach (var property in properties)
        {
            if (objects.IsUnknown)
            {
                return ObjectSet.Unknown;
            }

            objects = objects.Objects.Aggregate(
                ObjectSet.Empty,
                (result, owner) => result.Union(ReadEdge(new Edge(owner, property))));
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
        => new(variables.SetItem(variable, value), edges, propertiesAssignedOnUnknown);

    /// <summary>
    /// Returns the picture after <c>receiver.Property = value</c>. A sure receiver has its edge replaced. Several
    /// receivers each may or may not have been assigned, so the value is added to what their edges held.
    /// </summary>
    /// <param name="receiver">Objects of the expression before the property.</param>
    /// <param name="property">Assigned property.</param>
    /// <param name="value">Objects of the assigned value.</param>
    /// <returns>New picture.</returns>
    public AliasState AssignEdge(ObjectSet receiver, IPropertySymbol property, ObjectSet value)
    {
        if (receiver.IsUnknown)
        {
            // Any object may be the receiver, so every edge with this property may now hold the value.
            var changedEdges = edges
                .Where(entry => SymbolEqualityComparer.Default.Equals(entry.Key.Property, property))
                .Select(entry => new KeyValuePair<Edge, ObjectSet>(
                    entry.Key,
                    entry.Value.Union(value).Union(ObjectSet.Unknown)));

            return new(variables, edges.SetItems(changedEdges), propertiesAssignedOnUnknown.Add(property));
        }

        if (receiver.IsSure)
        {
            var assignedEdge = new Edge(receiver.Objects.First(), property);

            return new(variables, edges.SetItem(assignedEdge, value), propertiesAssignedOnUnknown);
        }

        var addedEdges = receiver.Objects
            .Select(owner => new Edge(owner, property))
            .Select(edge => new KeyValuePair<Edge, ObjectSet>(edge, ReadEdge(edge).Union(value)));

        return new(variables, edges.SetItems(addedEdges), propertiesAssignedOnUnknown);
    }

    /// <summary>
    /// Returns the picture where two branches meet: each variable and edge holds what it holds in either one.
    /// </summary>
    /// <param name="other">Picture of the other branch.</param>
    /// <returns>Combined picture.</returns>
    public AliasState Combine(AliasState other)
    {
        // A variable assigned on one branch only cannot be read after the branches meet, since C# requires it to be
        // assigned on every branch, so it keeps the one value it has.
        var combinedVariables = other.variables.Aggregate(
            variables,
            (result, entry) => result.SetItem(
                entry.Key,
                result.TryGetValue(entry.Key, out var objects) ? objects.Union(entry.Value) : entry.Value));

        // An edge one branch has no entry for still holds its initial object there, which ReadEdge returns.
        var combinedEdges = edges.Keys
            .Concat(other.edges.Keys)
            .Distinct()
            .Select(edge => new KeyValuePair<Edge, ObjectSet>(edge, ReadEdge(edge).Union(other.ReadEdge(edge))));

        return new(
            combinedVariables,
            ImmutableDictionary<Edge, ObjectSet>.Empty.SetItems(combinedEdges),
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
           AreEqual(variables, other.variables) &&
           AreEqual(edges, other.edges) &&
           propertiesAssignedOnUnknown.SetEquals(other.propertiesAssignedOnUnknown);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AliasState);

    /// <inheritdoc />
    public override int GetHashCode()
        => unchecked((variables.Count * 31) + edges.Count + propertiesAssignedOnUnknown.Count);

    private static bool AreEqual<TKey>(
        ImmutableDictionary<TKey, ObjectSet> first,
        ImmutableDictionary<TKey, ObjectSet> second)
        where TKey : notnull
        => first.Count == second.Count &&
           first.All(entry => second.TryGetValue(entry.Key, out var objects) && objects.Equals(entry.Value));
}
