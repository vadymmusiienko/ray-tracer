using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a face in an object represented by three vertices and three normals
    /// This class does not inherit from "SceneEntity" because it's only a subclass for ObjModel
    /// This class shouldn't ever be accessed from anywhere exept from ObjModel
    /// </summary>
    class BVHTriangle
    {
        // Vertices of the face
        private Vector3 v0, v1, v2;

        // Getters for vertices (for BVH)
        public Vector3 V0 => this.v0;
        public Vector3 V1 => this.v1;
        public Vector3 V2 => this.v2;

        // Texture coordinates
        private TextureCoord? uv0, uv1, uv2;

        // Normals (for smooth shading)
        private Vector3? n0, n1, n2;
        private Material material;

        // Return center of the triangle (For BVH)
        public Vector3 Center => (v0 + v1 + v2) / 3.0;

        public BVHTriangle(Vector3 v0, Vector3 v1, Vector3 v2,
                       TextureCoord? uv0, TextureCoord? uv1, TextureCoord? uv2,
                       Vector3? n0, Vector3? n1, Vector3? n2, Material material)
        {
            // Initiate instance variables
            this.v0 = v0; this.v1 = v1; this.v2 = v2;
            this.uv0 = uv0; this.uv1 = uv1; this.uv2 = uv2;
            this.n0 = n0; this.n1 = n1; this.n2 = n2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {

            // Find a normal (perpendicular to the triangle)
            Vector3 normal = (v1 - v0).Cross(v2 - v1).Normalized();

            // Denom
            double denom = ray.Direction.Dot(normal);

            // Parallel - doens't intersect (1e-6 is basically 0)
            if (Math.Abs(denom) < 1e-6)
            {
                return null;
            }

            // Nom
            double nom = normal.Dot(v0 - ray.Origin); // Using v0 as center (doen't matter as long as the point lies on the plane)

            // t - distance from ray's origin to the hit point
            double t = nom / denom;

            // The hit point is behind ray origin (t < 0)
            if (t < 1e-6)
            {
                return null;
            }

            // Find the hitpoint
            Vector3 hitPoint = ray.Origin + ray.Direction * t;

            // Check if the hitpoint is inside the triangle using inside–outside test
            Vector3 edge0 = v1 - v0;
            Vector3 vp0 = hitPoint - v0;
            if (normal.Dot(edge0.Cross(vp0)) < 0)
                return null;

            Vector3 edge1 = v2 - v1;
            Vector3 vp1 = hitPoint - v1;
            if (normal.Dot(edge1.Cross(vp1)) < 0)
                return null;

            Vector3 edge2 = v0 - v2;
            Vector3 vp2 = hitPoint - v2;
            if (normal.Dot(edge2.Cross(vp2)) < 0)
                return null;

            // !Inside of the triangle if reached this point (passed 3 tests)!

            // Calculate barycentric coordinates to go from 3d to 2d (for procedural material and for smooth shading)

            // Calculate the area of the main triangle
            double mainTriangleArea = (v1 - v0).Cross(v2 - v0).Length();

            // Calculate areas of sub-triangles
            double area0 = (v1 - hitPoint).Cross(v2 - hitPoint).Length();  // Area opposite to v0
            double area1 = (v2 - hitPoint).Cross(v0 - hitPoint).Length();  // Area opposite to v1  
            double area2 = (v0 - hitPoint).Cross(v1 - hitPoint).Length();  // Area opposite to v2

            // Calculate barycentric coordinates (weights for each vertex)
            double bary0 = area0 / mainTriangleArea;  // Weight for v0
            double bary1 = area1 / mainTriangleArea;  // Weight for v1
            double bary2 = area2 / mainTriangleArea;  // Weight for v2

            // Interpolate normal for smooth shading
            Vector3 hitNormal = normal;
            if (n0.HasValue && n1.HasValue && n2.HasValue)
            {
                hitNormal = (bary0 * n0.Value + bary1 * n1.Value + bary2 * n2.Value).Normalized();
            }

            // Flip the normal if needed
            if (ray.Direction.Dot(hitNormal) > 0)
            {
                hitNormal = -hitNormal;
            }

            // Interpolate texture coordinates
            TextureCoord? hitUV = null;
            if (uv0.HasValue && uv1.HasValue && uv2.HasValue)
            {
                double texU = bary0 * uv0.Value.U + bary1 * uv1.Value.U + bary2 * uv2.Value.U;
                double texV = bary0 * uv0.Value.V + bary1 * uv1.Value.V + bary2 * uv2.Value.V;
                hitUV = new TextureCoord(texU, texV);
            }

            // Find the incident
            Vector3 incident = -ray.Direction;

            // Create Rayhit object
            RayHit hit;
            if (hitUV.HasValue)
            {
                hit = new RayHit(hitPoint, hitNormal, incident, this.material, hitUV);
            }
            else
            {
                hit = new RayHit(hitPoint, hitNormal, incident, this.material);

            }

            return hit;
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}