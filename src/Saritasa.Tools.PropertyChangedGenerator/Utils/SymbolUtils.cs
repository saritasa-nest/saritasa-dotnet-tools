using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.PropertyChangedGenerator.Utils;

/// <summary>
/// Utils for <see cref="ISymbol"/>.
/// </summary>
internal static class SymbolUtils
{
    private static readonly Accessibility[] baseClassModifiers = [Accessibility.Public, Accessibility.Protected];

    /// <summary>
    /// Gets a symbol containing namespace.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <returns>Containing namespace.</returns>
    public static string GetNamespace(ISymbol symbol)
        => symbol.ContainingNamespace.ToString();

    /// <summary>
    /// Gets an access modifier.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <returns>Access modifier.</returns>
    public static string GetModifier(ISymbol symbol)
        => symbol.DeclaredAccessibility.ToString().ToLower();

    /// <summary>
    /// Gets a property type.
    /// </summary>
    /// <param name="symbol">Property symbol.</param>
    /// <returns>Property type.</returns>
    public static string GetType(IPropertySymbol symbol)
        => symbol.Type.ToDisplayString();

    /// <summary>
    /// Gets a field type.
    /// </summary>
    /// <param name="symbol">Field symbol.</param>
    /// <returns>Field type.</returns>
    public static string GetType(IFieldSymbol symbol)
        => symbol.Type.ToDisplayString();

    /// <summary>
    /// Indicates if symbol contains an attribute.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <param name="attributeName">Attribute name.</param>
    /// <returns>True, if symbol contains attribute.</returns>
    public static bool ContainsAttribute(ISymbol symbol, string attributeName)
        => symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == attributeName);

    /// <summary>
    /// Returns an attribute.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <param name="attributeName">Attribute name.</param>
    /// <returns>Attribute data.</returns>
    public static AttributeData GetAttribute(ISymbol symbol, string attributeName)
        => symbol.GetAttributes().First(attr => attr.AttributeClass?.Name == attributeName);

    /// <summary>
    /// Return an attributes.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <param name="attributeName">Attribute name.</param>
    /// <returns>Attributes data.</returns>
    public static IEnumerable<AttributeData> GetAttributes(ISymbol symbol, string attributeName)
        => symbol.GetAttributes().Where(attr => attr.AttributeClass?.Name == attributeName);

    /// <summary>
    /// Find an event raise method.
    /// </summary>
    /// <param name="symbol">Event symbol.</param>
    /// <param name="methodNames">Available method names.</param>
    /// <returns>Method symbol.</returns>
    public static IMethodSymbol? FindRaiseMethod(ITypeSymbol symbol, string[] methodNames)
    {
        var classes = GetBaseSymbols(symbol);
        if (!classes.Any())
        {
            return default;
        }

        return classes
            .SelectMany(@class => @class.GetMembers())
            .OfType<IMethodSymbol>()
            .Where(method => method.MethodKind == MethodKind.Ordinary)
            .FirstOrDefault(method => methodNames.Contains(method.Name));
    }

    /// <summary>
    /// Get descendant base symbol types.
    /// </summary>
    /// <param name="symbol">Type symbol.</param>
    /// <returns>List of base type symbols.</returns>
    public static IEnumerable<ITypeSymbol> GetBaseSymbols(ITypeSymbol symbol)
    {
        if (symbol.BaseType != null)
        {
            foreach (var inner in GetBaseSymbols(symbol.BaseType))
            {
                yield return inner;
            }
        }

        yield return symbol;
    }

    /// <summary>
    /// Gets symbol member.
    /// </summary>
    /// <param name="symbol">Type symbol.</param>
    /// <param name="isBase">Should traverse base class.</param>
    /// <returns>List of members.</returns>
    public static IEnumerable<ISymbol> GetMembers(ITypeSymbol symbol, bool isBase = false)
    {
        if (symbol.BaseType != null)
        {
            foreach (var member in GetMembers(symbol.BaseType, isBase: true))
            {
                yield return member;
            }
        }

        if (isBase)
        {
            foreach (var member in symbol.GetMembers())
            {
                if (baseClassModifiers.Contains(member.DeclaredAccessibility))
                {
                    yield return member;
                }
            }
        }
        else
        {
            foreach (var member in symbol.GetMembers())
            {
                yield return member;
            }
        }
    }

    /// <summary>
    /// Indicates if type symbol contains base list symbol.
    /// </summary>
    /// <param name="symbol">Type symbol.</param>
    /// <param name="containing">Containing symbol.</param>
    /// <returns>True, if type symbol contains in base list.</returns>
    public static bool ContainsBaseList(ITypeSymbol symbol, ISymbol containing)
        => GetBaseSymbols(symbol).Any(s => s.Equals(containing, SymbolEqualityComparer.Default));
}
