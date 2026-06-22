using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RayTracer.Json
{
    /// <summary>
    /// Maps short, user-facing asset keys (e.g. "checkers", "bunny") to
    /// absolute file paths on the server. The web tier ONLY ever accepts asset
    /// keys, never raw paths, so a JSON scene can never trigger an arbitrary
    /// file read. Unknown keys are rejected.
    /// </summary>
    public sealed class AssetResolver
    {
        private readonly Dictionary<string, string> textures;
        private readonly Dictionary<string, string> models;

        /// <param name="textures">key -> absolute path of allowed texture images</param>
        /// <param name="models">key -> absolute path of allowed .obj models</param>
        public AssetResolver(Dictionary<string, string> textures, Dictionary<string, string> models)
        {
            this.textures = textures ?? new Dictionary<string, string>();
            this.models = models ?? new Dictionary<string, string>();
        }

        /// <summary>An empty resolver that rejects every asset key.</summary>
        public static AssetResolver Empty { get; } =
            new AssetResolver(new Dictionary<string, string>(), new Dictionary<string, string>());

        /// <summary>Keys of every allowed texture, for the capabilities endpoint.</summary>
        public IReadOnlyCollection<string> TextureKeys => this.textures.Keys.ToList();

        /// <summary>Keys of every allowed model, for the capabilities endpoint.</summary>
        public IReadOnlyCollection<string> ModelKeys => this.models.Keys.ToList();

        public bool TryResolveTexture(string key, out string path) =>
            this.textures.TryGetValue(key ?? string.Empty, out path);

        public bool TryResolveModel(string key, out string path) =>
            this.models.TryGetValue(key ?? string.Empty, out path);

        /// <summary>
        /// Build a resolver by scanning a textures directory (png/jpg/jpeg) and
        /// a models directory (.obj). The asset key is the file name without
        /// extension. Missing directories yield no assets rather than throwing.
        /// </summary>
        public static AssetResolver FromDirectories(string texturesDir, string modelsDir)
        {
            var textures = ScanDirectory(texturesDir, new[] { ".png", ".jpg", ".jpeg" });
            var models = ScanDirectory(modelsDir, new[] { ".obj" });
            return new AssetResolver(textures, models);
        }

        private static Dictionary<string, string> ScanDirectory(string dir, string[] extensions)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return map;
            }
            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (!extensions.Contains(ext))
                {
                    continue;
                }
                var key = Path.GetFileNameWithoutExtension(file);
                // First definition wins; avoids surprises from duplicate stems.
                if (!map.ContainsKey(key))
                {
                    map[key] = Path.GetFullPath(file);
                }
            }
            return map;
        }
    }
}
