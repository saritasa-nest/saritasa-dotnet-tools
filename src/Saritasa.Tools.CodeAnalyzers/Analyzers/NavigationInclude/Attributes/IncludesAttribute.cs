namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

/// <summary>
/// Declares that a method guarantees a specific navigation property is loaded on its
/// return value. Paired with <see cref="IncludeRequiredAttribute"/>: when a caller
/// receives the return value and passes it to a method with
/// <c>[IncludeRequired(param, propertyName)]</c>, the analyzer treats the value as
/// already having the property loaded (suppressing INCL002).
/// </summary>
/// <remarks>
/// The <c>NavigationIncludeAnalyzer</c> also enforces this contract: if the method body
/// returns a value that does not load the declared property via <c>.Include()</c>, an
/// object initializer, or another <c>[Includes]</c> method, INCL003 is reported.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class IncludesAttribute : Attribute
{
    /// <summary>
    /// Name of the navigation property that is guaranteed to be loaded on the return value.
    /// Must match the property name exactly (case-sensitive).
    /// </summary>
    public string IncludedProperty { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="includedProperty">
    /// Name of the navigation property guaranteed to be loaded on the return value.
    /// </param>
    public IncludesAttribute(string includedProperty)
    {
        IncludedProperty = includedProperty;
    }
}
