using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;

namespace Saritasa.Tools.PropertyChangedGenerator.Utils;

/// <summary>
/// Utilities for naming conventions.
/// </summary>
internal static class NamingUtils
{
    /// <summary>
    /// Convert a value to naming convention.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <param name="convention">Naming convention.</param>
    /// <returns>Converted value.</returns>
    public static string ToConvention(string value, NamingConvention convention) => convention switch
    {
        NamingConvention.PascalCase => Capitalize(value),
        NamingConvention.CamelCase => Lowercase(value),
        _ => throw new ArgumentOutOfRangeException(nameof(NamingConvention), "Invalid naming convention."),
    };

    /// <summary>
    /// Indicates whether value follow convention.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="convention">Naming convention.</param>
    /// <returns>True, if value follow convention.</returns>
    public static bool FollowConvention(string value, NamingConvention convention) => convention switch
    {
        NamingConvention.PascalCase => char.IsUpper(value[0]),
        NamingConvention.CamelCase => char.IsLower(value[0]),
        _ => throw new ArgumentOutOfRangeException(nameof(NamingConvention), "Invalid naming convention."),
    };

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
    private static string Lowercase(string value) => char.ToLower(value[0]) + value.Substring(1);
}
