using System.Collections.Generic;
using RayTracer.Json;

namespace RayTracer.Web;

/// <summary>
/// Caps for the synchronous "preview" render tier so a single web request
/// returns in a few seconds and can't exhaust the server. Hard limits
/// (dimensions, sample count, object counts) are rejected with a 400 so the UI
/// is honest about what it asked for; features not offered in the sync tier
/// (animation, depth of field) are quietly forced to safe values.
/// </summary>
public sealed class PreviewLimits
{
    public int MaxWidth { get; init; } = 600;
    public int MaxHeight { get; init; } = 600;
    public int MaxAaMultiplier { get; init; } = 2;
    public int MaxObjects { get; init; } = 200;
    public int MaxLights { get; init; } = 16;
    public int MaxMaterials { get; init; } = 64;
    public int TimeoutMs { get; init; } = 20000;

    /// <summary>-1 means use all cores.</summary>
    public int MaxDegreeOfParallelism { get; init; } = -1;

    /// <summary>
    /// Validate a scene against the hard caps, collecting every violation, and
    /// force unsupported features (multi-frame, DOF) to safe preview values.
    /// Returns the list of hard-cap errors (empty when acceptable).
    /// </summary>
    public List<string> EnforceOnto(SceneDto dto)
    {
        var errors = new List<string>();
        dto.Render ??= new RenderDto();
        RenderDto r = dto.Render;

        if ((r.Width ?? 400) > MaxWidth)
        {
            errors.Add($"render.width exceeds the preview maximum of {MaxWidth}.");
        }
        if ((r.Height ?? 400) > MaxHeight)
        {
            errors.Add($"render.height exceeds the preview maximum of {MaxHeight}.");
        }
        if ((r.AaMultiplier ?? 1) > MaxAaMultiplier)
        {
            errors.Add($"render.aaMultiplier exceeds the preview maximum of {MaxAaMultiplier}.");
        }
        if ((dto.Objects?.Count ?? 0) > MaxObjects)
        {
            errors.Add($"Too many objects ({dto.Objects!.Count}); preview maximum is {MaxObjects}.");
        }
        if ((dto.Lights?.Count ?? 0) > MaxLights)
        {
            errors.Add($"Too many lights ({dto.Lights!.Count}); preview maximum is {MaxLights}.");
        }
        if ((dto.Materials?.Count ?? 0) > MaxMaterials)
        {
            errors.Add($"Too many materials ({dto.Materials!.Count}); preview maximum is {MaxMaterials}.");
        }

        // Features not available in the synchronous preview tier: force them off
        // rather than erroring (the UI never exposes them).
        r.FrameCount = 1;
        r.ApertureRadius = 0;

        return errors;
    }
}
