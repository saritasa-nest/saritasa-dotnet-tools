using Saritasa.Tools.SourceGenerator.Models.Descriptors;

namespace Saritasa.Tools.SourceGenerator.Diagnostics;

/// <summary>
/// Contains an application diagnostics.
/// </summary>
internal class Diagnostics
{
    /// <summary>
    /// Backing field convention mismatch warning.
    /// </summary>
    public static readonly WarningDescriptor BackingFieldConventionMismatch = new(
        title: "Backing field options mismatch.",
        message: "Backing field PascalCase naming convention cannot be used without underscore.",
        code: "ST01",
        format: string.Empty);
}
