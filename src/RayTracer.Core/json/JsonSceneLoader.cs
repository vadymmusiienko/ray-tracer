using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RayTracer.Json
{
    /// <summary>
    /// The result of loading a JSON scene: a fully populated <see cref="Scene"/>
    /// plus the output dimensions and animation time (which live outside the
    /// Scene object, since Render takes them via the Image and a time argument).
    /// </summary>
    public sealed class SceneBuildResult
    {
        public Scene Scene { get; }
        public int Width { get; }
        public int Height { get; }
        public double Time { get; }

        public SceneBuildResult(Scene scene, int width, int height, double time)
        {
            this.Scene = scene;
            this.Width = width;
            this.Height = height;
            this.Time = time;
        }
    }

    /// <summary>
    /// Builds a <see cref="Scene"/> from a <see cref="SceneDto"/>, mirroring what
    /// SceneReader.PopulateScene does but from JSON. It reuses the exact same
    /// scene-object constructors, so a JSON scene renders identically to the
    /// equivalent text scene. All problems are collected and reported together
    /// via <see cref="SceneValidationException"/>.
    /// </summary>
    public static class JsonSceneLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        /// <summary>
        /// Deserialize a JSON string into a <see cref="SceneDto"/>. Throws
        /// <see cref="SceneValidationException"/> on malformed JSON.
        /// </summary>
        public static SceneDto Deserialize(string json)
        {
            try
            {
                var dto = JsonSerializer.Deserialize<SceneDto>(json, JsonOptions);
                if (dto == null)
                {
                    throw new SceneValidationException(new[] { "Scene body was empty." });
                }
                return dto;
            }
            catch (JsonException ex)
            {
                throw new SceneValidationException(new[] { "Malformed JSON: " + ex.Message });
            }
        }

        /// <summary>Convenience: deserialize then build.</summary>
        public static SceneBuildResult Load(string json, AssetResolver assets) =>
            Load(Deserialize(json), assets);

        /// <summary>
        /// Build a scene from an already-deserialized DTO. <paramref name="assets"/>
        /// may be null, in which case any texture/OBJ reference is an error.
        /// </summary>
        public static SceneBuildResult Build(SceneDto dto, AssetResolver assets) => Load(dto, assets);

        public static SceneBuildResult Load(SceneDto dto, AssetResolver assets)
        {
            assets = assets ?? AssetResolver.Empty;
            var errors = new List<string>();

            // ---- Render options (with engine defaults) ----------------------
            RenderDto r = dto.Render ?? new RenderDto();
            int width = ValidatePositive(r.Width ?? 400, "render.width", errors);
            int height = ValidatePositive(r.Height ?? 400, "render.height", errors);
            int aa = r.AaMultiplier ?? 1;
            if (aa < 1) { errors.Add("render.aaMultiplier must be at least 1."); aa = 1; }
            double aperture = r.ApertureRadius ?? 0;
            if (aperture < 0) { errors.Add("render.apertureRadius cannot be negative."); aperture = 0; }
            double focal = r.FocalLength ?? 1;
            int quality = r.Quality ?? 0;
            int frameCount = r.FrameCount ?? 1;
            if (frameCount < 1) { errors.Add("render.frameCount must be at least 1."); frameCount = 1; }
            int fps = r.FramesPerSecond ?? 30;
            double time = r.Time ?? 0;
            bool ambient = r.AmbientLighting ?? false;

            var options = new SceneOptions(aa, ambient, aperture, focal, quality, frameCount, fps);

            // ---- Materials --------------------------------------------------
            var materials = new Dictionary<string, Material>();
            foreach (var m in dto.Materials ?? new List<MaterialDto>())
            {
                if (string.IsNullOrWhiteSpace(m.Id))
                {
                    errors.Add("A material is missing its 'id'.");
                    continue;
                }
                if (materials.ContainsKey(m.Id))
                {
                    errors.Add($"Duplicate material id '{m.Id}'.");
                    continue;
                }
                Material built = BuildMaterial(m, assets, errors);
                if (built != null)
                {
                    materials[m.Id] = built;
                }
                else
                {
                    // Register a placeholder so object references don't double-report.
                    materials[m.Id] = null;
                }
            }

            // ---- Camera -----------------------------------------------------
            Camera camera = null;
            if (dto.Camera == null)
            {
                errors.Add("A camera is required.");
            }
            else
            {
                Transform t = BuildTransform(
                    dto.Camera.Position, dto.Camera.RotationAxis,
                    dto.Camera.RotationAngle, dto.Camera.Scale, "camera", errors);
                camera = new Camera(t);
            }

            // ---- Lights -----------------------------------------------------
            var lights = new List<PointLight>();
            int li = 0;
            foreach (var l in dto.Lights ?? new List<LightDto>())
            {
                string ctx = $"lights[{li}]";
                Vector3 pos = ToVector3(l.Position, $"{ctx}.position", errors);
                Color col = ToColor(l.Color, $"{ctx}.color", errors);
                lights.Add(new PointLight(pos, col));
                li++;
            }

            // ---- Validate objects (without expensive construction yet) ------
            int oi = 0;
            foreach (var o in dto.Objects ?? new List<ObjectDto>())
            {
                ValidateObject(o, $"objects[{oi}]", materials, assets, errors);
                oi++;
            }

            if (errors.Count > 0)
            {
                throw new SceneValidationException(errors);
            }

            // ---- Assemble (everything validated; safe to build heavy parts) -
            var scene = new Scene(options);
            scene.SetCamera(camera);
            foreach (var light in lights)
            {
                scene.AddPointLight(light);
            }
            oi = 0;
            foreach (var o in dto.Objects ?? new List<ObjectDto>())
            {
                scene.AddEntity(BuildEntity(o, $"objects[{oi}]", materials, assets));
                oi++;
            }

            return new SceneBuildResult(scene, width, height, time);
        }

        // ----------------------------------------------------------------------
        // Material construction
        // ----------------------------------------------------------------------
        private static Material BuildMaterial(MaterialDto m, AssetResolver assets, List<string> errors)
        {
            string type = (m.Type ?? "standard").ToLowerInvariant();
            string ctx = $"material '{m.Id}'";
            switch (type)
            {
                case "standard":
                    return new Material(
                        ToColor(m.Ambient, $"{ctx}.ambient", errors),
                        ToColor(m.Diffuse, $"{ctx}.diffuse", errors),
                        ToColor(m.Specular, $"{ctx}.specular", errors),
                        m.Shininess ?? 0,
                        m.Reflectivity ?? 0,
                        m.Transmissivity ?? 0,
                        m.RefractiveIndex ?? 1);

                case "texture":
                    {
                        if (!assets.TryResolveTexture(m.ColorMap, out string colorPath))
                        {
                            errors.Add($"{ctx}: unknown texture asset '{m.ColorMap}' for colorMap.");
                            return null;
                        }
                        Image colorMap = Image.LoadFromFile(colorPath);
                        Image normalMap = null;
                        if (!string.IsNullOrEmpty(m.NormalMap))
                        {
                            if (!assets.TryResolveTexture(m.NormalMap, out string normalPath))
                            {
                                errors.Add($"{ctx}: unknown texture asset '{m.NormalMap}' for normalMap.");
                                return null;
                            }
                            normalMap = Image.LoadFromFile(normalPath);
                        }
                        return new TextureMaterial(colorMap, normalMap);
                    }

                case "procedural":
                    {
                        if (!Enum.TryParse(m.Pattern, true, out ProceduralMaterial.PatternType pattern))
                        {
                            errors.Add($"{ctx}: unknown pattern '{m.Pattern}' (expected Checkers or Stripes).");
                            return null;
                        }
                        return new ProceduralMaterial(
                            pattern,
                            m.ScaleU ?? 1,
                            m.ScaleV ?? 1,
                            ToColor(m.Color1, $"{ctx}.color1", errors),
                            ToColor(m.Color2, $"{ctx}.color2", errors));
                    }

                default:
                    errors.Add($"{ctx}: unknown material type '{m.Type}'.");
                    return null;
            }
        }

        // ----------------------------------------------------------------------
        // Object validation + construction
        // ----------------------------------------------------------------------
        private static void ValidateObject(
            ObjectDto o, string ctx, Dictionary<string, Material> materials,
            AssetResolver assets, List<string> errors)
        {
            string type = (o.Type ?? "").ToLowerInvariant();

            // Material reference (every object type requires one).
            if (string.IsNullOrWhiteSpace(o.Material))
            {
                errors.Add($"{ctx}: missing 'material'.");
            }
            else if (!materials.TryGetValue(o.Material, out Material mat) || mat == null)
            {
                if (!materials.ContainsKey(o.Material))
                {
                    errors.Add($"{ctx}: references undefined material '{o.Material}'.");
                }
                // if it exists but is null, the material itself already errored.
            }

            switch (type)
            {
                case "sphere":
                    Require(o.Center, $"{ctx}.center", errors);
                    if (o.Radius == null) errors.Add($"{ctx}: missing 'radius'.");
                    else if (o.Radius <= 0) errors.Add($"{ctx}: 'radius' must be positive.");
                    break;
                case "plane":
                    Require(o.Point, $"{ctx}.point", errors);
                    Require(o.Normal, $"{ctx}.normal", errors);
                    break;
                case "triangle":
                    Require(o.V0, $"{ctx}.v0", errors);
                    Require(o.V1, $"{ctx}.v1", errors);
                    Require(o.V2, $"{ctx}.v2", errors);
                    break;
                case "objmodel":
                    if (!assets.TryResolveModel(o.Model, out _))
                    {
                        errors.Add($"{ctx}: unknown model asset '{o.Model}'.");
                    }
                    break;
                case "":
                    errors.Add($"{ctx}: missing 'type'.");
                    break;
                default:
                    errors.Add($"{ctx}: unknown object type '{o.Type}'.");
                    break;
            }
        }

        private static SceneEntity BuildEntity(
            ObjectDto o, string ctx, Dictionary<string, Material> materials, AssetResolver assets)
        {
            Material mat = materials[o.Material];
            var unused = new List<string>(); // validation passed; vectors are valid
            switch (o.Type.ToLowerInvariant())
            {
                case "sphere":
                    return new Sphere(ToVector3(o.Center, ctx, unused), o.Radius.Value, mat);
                case "plane":
                    return new Plane(ToVector3(o.Point, ctx, unused), ToVector3(o.Normal, ctx, unused), mat);
                case "triangle":
                    return new Triangle(
                        ToVector3(o.V0, ctx, unused), ToVector3(o.V1, ctx, unused),
                        ToVector3(o.V2, ctx, unused), mat);
                case "objmodel":
                    assets.TryResolveModel(o.Model, out string modelPath);
                    Transform t = BuildTransform(
                        o.Position, o.RotationAxis, o.RotationAngle, o.Scale, ctx, unused);
                    return new ObjModel(modelPath, t, mat);
                default:
                    throw new InvalidOperationException($"{ctx}: unreachable object type.");
            }
        }

        // ----------------------------------------------------------------------
        // Conversion helpers
        // ----------------------------------------------------------------------
        private static Transform BuildTransform(
            double[] position, double[] axis, double? angle, double? scale,
            string ctx, List<string> errors)
        {
            Vector3 pos = position == null ? new Vector3(0, 0, 0) : ToVector3(position, $"{ctx}.position", errors);
            Vector3 ax = axis == null ? new Vector3(0, 0, 1) : ToVector3(axis, $"{ctx}.rotationAxis", errors);
            var rotation = new Quaternion(ax, angle ?? 0);
            return new Transform(pos, rotation, scale ?? 1);
        }

        private static Vector3 ToVector3(double[] a, string field, List<string> errors)
        {
            if (a == null || a.Length != 3)
            {
                errors.Add($"{field} must be a 3-element array [x, y, z].");
                return new Vector3(0, 0, 0);
            }
            return new Vector3(a[0], a[1], a[2]);
        }

        private static Color ToColor(double[] a, string field, List<string> errors)
        {
            if (a == null || a.Length != 3)
            {
                errors.Add($"{field} must be a 3-element array [r, g, b].");
                return new Color(0, 0, 0);
            }
            return new Color(a[0], a[1], a[2]);
        }

        private static int ValidatePositive(int value, string field, List<string> errors)
        {
            if (value <= 0)
            {
                errors.Add($"{field} must be positive.");
                return 1;
            }
            return value;
        }

        private static void Require(double[] a, string field, List<string> errors)
        {
            if (a == null || a.Length != 3)
            {
                errors.Add($"{field} must be a 3-element array.");
            }
        }
    }
}
