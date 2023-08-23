using Saritasa.Tools.SourceGenerator.Infrastructure.Options;

namespace Saritasa.Tools.SourceGenerator.Utils;

/// <summary>
/// Backing field utilities.
/// </summary>
internal class FieldUtils
{
    /// <summary>
    /// Gets possible field property name.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="options">Field options.</param>
    /// <returns>Field property name.</returns>
    public static string GetPropertyName(string fieldName, FieldOptions options)
    {
        if (options.UseUnderscore && fieldName.StartsWith("_"))
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

        var withoutUnderscore = fieldName.Remove(0, 1);
        return NamingUtils.FollowConvention(withoutUnderscore, options.Convention);
    }
}
