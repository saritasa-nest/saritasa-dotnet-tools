namespace Saritasa.Tools.SourceGenerator.Utils;

/// <summary>
/// Property utilities.
/// </summary>
internal class PropertyUtils
{
    /// <summary>
    /// Capitalize value.
    /// </summary>
    /// <param name="value">Value to capitalize.</param>
    /// <returns>Capitalized string.</returns>
    public static string Capitalize(string value) => char.ToUpper(value[0]) + value.Substring(1);

    /// <summary>
    /// Lowercase value.
    /// </summary>
    /// <param name="value">Value to lowercase.</param>
    /// <returns>Lowercased string.</returns>
    public static string Lowercase(string value) => char.ToLower(value[0]) + value.Substring(1);
}
