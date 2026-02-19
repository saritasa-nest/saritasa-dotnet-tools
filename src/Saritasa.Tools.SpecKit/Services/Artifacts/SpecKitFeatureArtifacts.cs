namespace Saritasa.Tools.SpecKit.Services.Artifacts;

/// <summary>
/// Spec Kit artifacts.
/// </summary>
/// <param name="Specification">Specification file.</param>
/// <param name="Plan">Plan file.</param>
/// <param name="Tasks">Tasks file.</param>
public record SpecKitFeatureArtifacts(
    SpecKitArtifact Specification,
    SpecKitArtifact Plan,
    SpecKitArtifact Tasks);
