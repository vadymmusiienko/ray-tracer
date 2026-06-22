using RayTracer;
using RayTracer.Json;
using RayTracer.Web;

var builder = WebApplication.CreateBuilder(args);

// Server-side asset allow-list: only these texture/model keys can ever be
// referenced by a scene, so user JSON can never trigger an arbitrary file read.
builder.Services.AddSingleton(_ => AssetResolver.FromDirectories(
    Path.Combine(builder.Environment.ContentRootPath, "assets", "textures"),
    Path.Combine(builder.Environment.ContentRootPath, "assets", "models")));
builder.Services.AddSingleton(new PreviewLimits());
builder.Services.AddSingleton<IRenderService, RenderService>();

// CORS for the Next.js frontend. In dev it proxies /api/* server-side (no CORS
// needed), but allowing the configured origins lets the browser also call the
// API directly when the frontend is deployed separately. Configure via the
// "Cors:AllowedOrigins" setting; defaults to the local Next dev server.
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

// --- POST /api/render : JSON scene -> PNG ---------------------------------
app.MapPost("/api/render", async (HttpRequest request, IRenderService renderer, CancellationToken ct) =>
{
    string body;
    using (var reader = new StreamReader(request.Body))
    {
        body = await reader.ReadToEndAsync(ct);
    }

    SceneDto dto;
    try
    {
        dto = JsonSceneLoader.Deserialize(body);
    }
    catch (SceneValidationException ex)
    {
        return Results.BadRequest(new { errors = ex.Errors });
    }

    try
    {
        RenderResult result = await renderer.RenderAsync(dto, ct);
        return Results.File(result.Png, "image/png");
    }
    catch (SceneValidationException ex)
    {
        return Results.BadRequest(new { errors = ex.Errors });
    }
    catch (OperationCanceledException)
    {
        return Results.Json(
            new { errors = new[] { "Render timed out or was cancelled." } },
            statusCode: StatusCodes.Status408RequestTimeout);
    }
    catch (Exception)
    {
        // Don't leak stack traces to the client.
        return Results.Json(
            new { error = "Internal render error." },
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// --- GET /api/capabilities : what the UI may offer + the server limits ----
app.MapGet("/api/capabilities", (AssetResolver assets, PreviewLimits limits) =>
    Results.Ok(new
    {
        materialTypes = new[] { "standard", "texture", "procedural" },
        patterns = new[] { "Checkers", "Stripes" },
        objectTypes = new[] { "sphere", "plane", "triangle", "objModel" },
        textures = assets.TextureKeys,
        models = assets.ModelKeys,
        limits = new
        {
            maxWidth = limits.MaxWidth,
            maxHeight = limits.MaxHeight,
            maxAaMultiplier = limits.MaxAaMultiplier,
            maxObjects = limits.MaxObjects,
            maxLights = limits.MaxLights,
            maxMaterials = limits.MaxMaterials,
        },
    }));

app.Run();
