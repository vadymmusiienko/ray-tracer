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
        public static List<ObjFace> Parse(string objFilePath, Transform transform, Material material)
        {
            // Store all vertices, normals and texture coordinates
            List<Vector3> vertices = new List<Vector3>(); // Mandatory
            List<Vector3> normals = new List<Vector3>(); // Optional
            List<TextureCoord> textureCoords = new List<TextureCoord>(); // Optional

            // Store obj object vertices (triangles)
            List<ObjFace> faces = new List<ObjFace>();

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
                    // Arrays to store all 3 points of v, vt, vn (-1 for missing values)
                    int[] vIndices = new int[3];
                    int[] vtIndices = new int[3];
                    int[] vnIndices = new int[3];

                    // Get indices (must be 3 - assuming triangles)
                    for (int j = 0; j < 3; j++)
                    {
                        // Get all tokens (v, vt (if present) and vn(if present))
                        string[] tokens = parts[j + 1].Split('/');

                        // Vertex index (always present)
                        vIndices[j] = int.Parse(tokens[0]) - 1;

                        // Texture coord (optional)
                        if (tokens.Length > 1 && tokens[1] != "")
                            vtIndices[j] = int.Parse(tokens[1]) - 1;
                        else
                            vtIndices[j] = -1; // Doesn't exist (-1)

                        // normal index (optional)
                        if (tokens.Length > 2 && tokens[2] != "")
                            vnIndices[j] = int.Parse(tokens[2]) - 1;
                        else
                            vnIndices[j] = -1; // Doesn't exist (-1)
                    }

                    // Collect vertices
                    Vector3 v0 = vertices[vIndices[0]];
                    Vector3 v1 = vertices[vIndices[1]];
                    Vector3 v2 = vertices[vIndices[2]];

                    // Collect texture coordinates (nullable)
                    TextureCoord? vt0 = vtIndices[0] >= 0 ? textureCoords[vtIndices[0]] : null;
                    TextureCoord? vt1 = vtIndices[1] >= 0 ? textureCoords[vtIndices[1]] : null;
                    TextureCoord? vt2 = vtIndices[2] >= 0 ? textureCoords[vtIndices[2]] : null;

                    // Collect normals
                    Vector3? vn0 = vnIndices[0] >= 0 ? normals[vnIndices[0]] : null;
                    Vector3? vn1 = vnIndices[1] >= 0 ? normals[vnIndices[1]] : null;
                    Vector3? vn2 = vnIndices[2] >= 0 ? normals[vnIndices[2]] : null;

                    // Create and add a face (triangle)
                    ObjFace face = new ObjFace(v0, v1, v2, vt0, vt1, vt2, vn0, vn1, vn2, material);
                    faces.Add(face);
                }

            }

            return faces;
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