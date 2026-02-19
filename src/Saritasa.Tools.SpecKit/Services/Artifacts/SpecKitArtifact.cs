namespace Saritasa.Tools.SpecKit.Services.Artifacts;

/// <summary>
/// Spec Kit artifact.
/// </summary>
/// <param name="Path">Path to artifact.</param>
/// <param name="IsExist">Is artifact exists.</param>
public record SpecKitArtifact(
    string Path,
    bool IsExist);
