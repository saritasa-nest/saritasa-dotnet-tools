using Saritasa.Tools.SourceGenerator.Models.Descriptors;

namespace Saritasa.Tools.SourceGenerator.Diagnostics;

/// <summary>
/// An application diagnostic descriptors.
/// </summary>
internal class DiagnosticDescriptors
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
