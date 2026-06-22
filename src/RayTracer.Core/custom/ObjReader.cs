using System.Collections.Generic;
using System.IO;

namespace RayTracer
{
    /// <summary>
    /// Static class to read/parse an OBJ file.
    /// </summary>
    static class ObjReader
    {
        /// <summary>
        /// Main static method to parse an OBJ file
        /// <param name="objFilePath">Path to the obj file to parse</param>
        /// <param name="transform">Transform object used to transform all the vertices</param>
        /// <param name="material">Material for the obj object</param>
        /// </summary> 
        public static BVH Parse(string objFilePath, Transform transform, Material material)
        {
            // Store all vertices, normals and texture coordinates
            List<Vector3> vertices = new List<Vector3>(); // Mandatory
            List<Vector3> normals = new List<Vector3>(); // Optional
            List<TextureCoord> textureCoords = new List<TextureCoord>(); // Optional

            // Store all indices
            List<int> vIndices = new List<int>();
            List<int> vtIndices = new List<int>();
            List<int> vnIndices = new List<int>();

            // Go through each line
            string[] lines = File.ReadAllLines(objFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                // Skip the comments and empty lines
                if (lines[i].StartsWith('#') || string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                // Split the line
                string[] parts = lines[i].Split(" "); // [v, -0.613298, 1.076979, 0.002881]

                // Get the identifier
                string identifier = parts[0]; // v or vn or f

                // Parse vertices (v)
                if (identifier == "v")
                {
                    Vector3 v = new Vector3(double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3]));
                    vertices.Add(ApplyTransform(v, transform));
                    continue;
                }

                // Parse texture coordinates (vt)
                if (identifier == "vt")
                {
                    // TextureCoords are 2D and don't have a "z" value, skip parts[3] - probably 0 anyway
                    TextureCoord vt = new TextureCoord(double.Parse(parts[1]), double.Parse(parts[2]));
                    textureCoords.Add(vt); // Don't need transformation
                    continue;
                }

                // Parse normals (vn)
                if (identifier == "vn")
                {
                    Vector3 normal = new Vector3(double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3]));
                    normals.Add(ApplyNormalTransform(normal, transform));
                    continue;
                }

                // Parse faces
                if (identifier == "f")
                {
                    for (int j = 0; j < 3; j++)
                    {
                        // Get all tokens (v, vt (if present) and vn(if present))
                        string[] tokens = parts[j + 1].Split('/');

                        // Vertex index (always present)
                        vIndices.Add(int.Parse(tokens[0]) - 1);

                        // Texture coord (optional)
                        if (tokens.Length > 1 && tokens[1] != "")
                        {

                            vtIndices.Add(int.Parse(tokens[1]) - 1);
                        }
                        // normal index (optional)
                        if (tokens.Length > 2 && tokens[2] != "")
                        {
                            vnIndices.Add(int.Parse(tokens[2]) - 1);
                        }
                    }
                }

            }

            // Construct a BVH tree
            BVH bvhTree = new BVH(vertices, normals, textureCoords, vIndices, vtIndices, vnIndices, material);
            return bvhTree;

        }

        private static Vector3 ApplyTransform(Vector3 vertex, Transform transform)
        {
            // Scale -> rotate -> translate
            Vector3 v = vertex * transform.Scale;
            v = transform.Rotation.Rotate(v);
            v += transform.Position;
            return v;
        }

        private static Vector3 ApplyNormalTransform(Vector3 normal, Transform transform)
        {
            Vector3 transformedNormal = transform.Rotation.Rotate(normal);
            return transformedNormal.Normalized();
        }
    }
}