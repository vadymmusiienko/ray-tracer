using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// </summary>
    public class ObjModel : SceneEntity
    {
        private string objFilePath;
        private Transform transform;
        private Material material;

        // vertices (all vertices as doubles from the obj file)
        List<Vector3> vertices = new List<Vector3>();

        // Vertex normals
        List<Vector3> normals = new List<Vector3>();

        // Faces (Triangles)
        // TODO: Extend the triangle class to accept 3 normals
        List<Triangle> faces = new List<Triangle>();

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="transform">Transform to apply to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Transform transform, Material material)
        {
            this.objFilePath = objFilePath;
            this.transform = transform;
            this.material = material;

            // Call the helper class to actually parse the file
            parseObjFile();
        }

        /// <summary>
        /// Helper method to parse an OBJ file
        /// <summary
        private void parseObjFile()
        {
            // Go through each line
            string[] lines = File.ReadAllLines(this.objFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                // !Note: v - vertices, vn - normals to those vertices, f - faces
                // Skip the comments
                if (lines[i].StartsWith('#'))
                {
                    continue;
                }

                // Split the line (4 parts)
                string[] parts = lines[i].Split(" "); // [v, -0.613298, 1.076979, 0.002881]

                // Unpack all the parts
                string identifier = parts[0]; // v or vn or f
                (string first, string second, string third) = (parts[1], parts[2], parts[3]); // -0.613298, 1.076979, 0.002881

                // Parse vertices (v)
                if (identifier == "v")
                {
                    Vector3 v = new Vector3(double.Parse(first), double.Parse(second), double.Parse(third));
                    this.vertices.Add(ApplyTransform(v));
                    continue;
                }

                // Parse normals (vn)
                if (identifier == "vn")
                {
                    // TODO : Transform normals
                    this.normals.Add(new Vector3(double.Parse(first), double.Parse(second), double.Parse(third)));
                    continue;
                }

                // Parse faces
                if (identifier == "f")
                {
                    // Find each vertex's index and its normal's index
                    // Split and deconstruct into vertex index and normal index
                    // !NOTE: This assumes this format "1013//1013 1014//1014 1015//1015", (just v and vn)
                    (int v0idx, int n0idx) = (int.Parse(parts[1].Split("//")[0]) - 1, int.Parse(parts[1].Split("//")[1]) - 1);
                    (int v1idx, int n1idx) = (int.Parse(parts[2].Split("//")[0]) - 1, int.Parse(parts[2].Split("//")[1]) - 1);
                    (int v2idx, int n2idx) = (int.Parse(parts[3].Split("//")[0]) - 1, int.Parse(parts[3].Split("//")[1]) - 1);

                    // TODO: Do something with normals (update the triangle class)
                    // ! I don't do anything with the normals yet

                    // Create and add a face (triangle)
                    Triangle triangle = new Triangle(vertices[v0idx], vertices[v1idx], vertices[v2idx], this.material);
                    faces.Add(triangle);
                }

            }
        }


        /// <summary>
        /// Apply Scale, rotation and translation (transform) a vertex
        /// </summary>
        /// <param name="vertex">Vertex to transform</param>
        /// <returns> Transformed Vector3 vertex </returns>
        private Vector3 ApplyTransform(Vector3 vertex)
        {
            // Scale -> rotate -> translate
            Vector3 v = vertex * this.transform.Scale;
            v = this.transform.Rotation.Rotate(v);
            v += this.transform.Position;
            return v;
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            // Find the first hit (if any)
            RayHit closestHit = null;
            // Triangle closestFace = null;
            double closestDistSq = double.MaxValue;

            foreach (Triangle face in this.faces)
            {
                RayHit hit = face.Intersect(ray);

                if (hit != null)
                {
                    double distSq = (hit.Position - ray.Origin).LengthSq();

                    // Check if this object is closer to the camera
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closestHit = hit;
                        // closestFace = face;
                    }
                }

            }

            // Return the closest hit
            return closestHit;
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}
