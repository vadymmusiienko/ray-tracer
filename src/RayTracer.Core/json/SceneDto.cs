using System.Collections.Generic;

namespace RayTracer.Json
{
    /// <summary>
    /// Top-level JSON scene description. This is the public, human-friendly
    /// input format that replaces the positional text DSL. It mirrors the data
    /// the old SceneReader collected, but as plain JSON the web UI can build.
    ///
    /// All vectors and colors are 3-element arrays: positions/normals are
    /// [x, y, z], colors are [r, g, b] in the 0..1 range. Materials and objects
    /// carry a "type" discriminator; only the fields relevant to that type are
    /// read, the rest may be omitted.
    /// </summary>
    public sealed class SceneDto
    {
        public RenderDto Render { get; set; }
        public CameraDto Camera { get; set; }
        public List<MaterialDto> Materials { get; set; } = new List<MaterialDto>();
        public List<LightDto> Lights { get; set; } = new List<LightDto>();
        public List<ObjectDto> Objects { get; set; } = new List<ObjectDto>();
    }

    /// <summary>Render settings. Omitted values fall back to engine defaults.</summary>
    public sealed class RenderDto
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? AaMultiplier { get; set; }
        public bool? AmbientLighting { get; set; }
        public double? ApertureRadius { get; set; }
        public double? FocalLength { get; set; }
        public int? Quality { get; set; }
        public int? FrameCount { get; set; }
        public int? FramesPerSecond { get; set; }
        public double? Time { get; set; }
    }

    /// <summary>Camera placement: position + axis-angle rotation + uniform scale.</summary>
    public sealed class CameraDto
    {
        public double[] Position { get; set; }
        public double[] RotationAxis { get; set; }
        public double? RotationAngle { get; set; }
        public double? Scale { get; set; }
    }

    /// <summary>
    /// A named material. "type" selects which fields are used:
    ///   "standard"   -> ambient, diffuse, specular, shininess, reflectivity,
    ///                   transmissivity, refractiveIndex
    ///   "texture"    -> colorMap (asset key), normalMap (asset key | null)
    ///   "procedural" -> pattern ("Checkers"|"Stripes"), scaleU, scaleV,
    ///                   color1, color2
    /// </summary>
    public sealed class MaterialDto
    {
        public string Id { get; set; }
        public string Type { get; set; }

        // standard
        public double[] Ambient { get; set; }
        public double[] Diffuse { get; set; }
        public double[] Specular { get; set; }
        public double? Shininess { get; set; }
        public double? Reflectivity { get; set; }
        public double? Transmissivity { get; set; }
        public double? RefractiveIndex { get; set; }

        // texture
        public string ColorMap { get; set; }
        public string NormalMap { get; set; }

        // procedural
        public string Pattern { get; set; }
        public double? ScaleU { get; set; }
        public double? ScaleV { get; set; }
        public double[] Color1 { get; set; }
        public double[] Color2 { get; set; }
    }

    /// <summary>A point light: world position + color.</summary>
    public sealed class LightDto
    {
        public double[] Position { get; set; }
        public double[] Color { get; set; }
    }

    /// <summary>
    /// A scene object referencing a material by id. "type" selects fields:
    ///   "sphere"   -> center, radius
    ///   "plane"    -> point, normal
    ///   "triangle" -> v0, v1, v2
    ///   "objModel" -> model (asset key), position, rotationAxis,
    ///                 rotationAngle, scale
    /// </summary>
    public sealed class ObjectDto
    {
        public string Type { get; set; }
        public string Material { get; set; }

        // sphere
        public double[] Center { get; set; }
        public double? Radius { get; set; }

        // plane
        public double[] Point { get; set; }
        public double[] Normal { get; set; }

        // triangle
        public double[] V0 { get; set; }
        public double[] V1 { get; set; }
        public double[] V2 { get; set; }

        // objModel
        public string Model { get; set; }
        public double[] Position { get; set; }
        public double[] RotationAxis { get; set; }
        public double? RotationAngle { get; set; }
        public double? Scale { get; set; }
    }
}
