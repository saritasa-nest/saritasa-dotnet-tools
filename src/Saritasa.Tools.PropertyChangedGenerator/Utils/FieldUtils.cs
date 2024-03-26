using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;

namespace Saritasa.Tools.PropertyChangedGenerator.Utils;

/// <summary>
/// Backing field utilities.
/// </summary>
internal static class FieldUtils
{
    /// <summary>
    /// Gets possible field property name.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="options">Field options.</param>
    /// <returns>Field property name.</returns>
    public static string GetPropertyName(string fieldName, FieldOptions options)
    {
        var isPascalCase = PascalCaseConvention(options);
        var isUnderscored = isPascalCase || options.UseUnderscore;
        if (isUnderscored && fieldName.StartsWith("_"))
        {
            fieldName = fieldName.Remove(0, 1);
        }

        return NamingUtils.Capitalize(fieldName);
    }

    /// <summary>
    /// Indicates whether field name follow convention.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="options">Field options.</param>
    /// <returns>True, if follow convention.</returns>
    public static bool FollowConvention(string fieldName, FieldOptions options)
    {
        if (options.UseUnderscore && !fieldName.StartsWith("_"))
        {
            return false;
        }

        // Pascal case backing fields must start with '_' character.
        var isPascalCase = PascalCaseConvention(options);
        if (isPascalCase && !fieldName.StartsWith("_"))
        {
            return false;
        }

        var name = options.UseUnderscore || isPascalCase ? fieldName.Remove(0, 1) : fieldName;
        return NamingUtils.FollowConvention(name, options.Convention);
    }

    private static bool PascalCaseConvention(FieldOptions options)
        => options.Convention == NamingConvention.PascalCase;
}
