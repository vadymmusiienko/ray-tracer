using System.Diagnostics;
using RayTracer.Json;

namespace RayTracer.Web;

/// <summary>The PNG bytes plus timing of a completed render.</summary>
public sealed record RenderResult(byte[] Png, int Width, int Height, long ElapsedMs);

/// <summary>
/// Renders a JSON scene to PNG bytes. Kept behind an interface so an async
/// job/queue tier can be layered on later without touching the engine or the
/// endpoints — both just depend on RenderAsync.
/// </summary>
public interface IRenderService
{
    Task<RenderResult> RenderAsync(SceneDto dto, CancellationToken cancellationToken);
}

public sealed class RenderService : IRenderService
{
    private readonly AssetResolver assets;
    private readonly PreviewLimits limits;

    public RenderService(AssetResolver assets, PreviewLimits limits)
    {
        this.assets = assets;
        this.limits = limits;
    }

    public async Task<RenderResult> RenderAsync(SceneDto dto, CancellationToken cancellationToken)
    {
        // Enforce preview caps first (throws 400-mapped exception on violation).
        var capErrors = limits.EnforceOnto(dto);
        if (capErrors.Count > 0)
        {
            throw new SceneValidationException(capErrors);
        }

        // Build + validate the scene (throws SceneValidationException on bad input).
        SceneBuildResult build = JsonSceneLoader.Load(dto, assets);

        // Bound the render with a hard deadline so a pathological scene can't
        // peg the CPU forever.
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(limits.TimeoutMs);
        CancellationToken token = timeoutCts.Token;

        // Render off the request thread (CPU-bound work).
        return await Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            var image = new Image(build.Width, build.Height);
            build.Scene.Render(
                image,
                build.Time,
                progress: null,
                cancellationToken: token,
                maxDegreeOfParallelism: limits.MaxDegreeOfParallelism);
            sw.Stop();
            return new RenderResult(image.EncodePNG(), build.Width, build.Height, sw.ElapsedMilliseconds);
        }, token);
    }
}
