using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a face in an object represented by three vertices and three normals
    /// This class does not inherit from "SceneEntity" because it's only a subclass for ObjModel
    /// This class shouldn't ever be accessed from anywhere exept from ObjModel
    /// </summary>
    class ObjFace
    {
        // Vertices of the face
        private Vector3 v0, v1, v2;

        // Texture coordinates
        private TextureCoord? uv0, uv1, uv2;

        // Normals (for smooth shading)
        private Vector3? n0, n1, n2;
        private Material material;

        public ObjFace(Vector3 v0, Vector3 v1, Vector3 v2,
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

            // TODO: Use all 3 normals for smooth shading
            // TODO: Find barycentric coordinates
            // TODO: Find a new normal for smooth shading
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

            // Inside of the triangle if reached this point (passed 3 tests)

            //!----------------------------------------------------
            // TODO: Is this how you support smooth shading?
            // Calculate barycentric coordinates to go from 3d to 2d (for procedural material and for smooth shading)
            edge1 = v2 - v0;
            Vector3 vp = hitPoint - v0;

            double d00 = edge0.Dot(edge0);
            double d01 = edge0.Dot(edge1);
            double d11 = edge1.Dot(edge1);
            double d20 = vp.Dot(edge0);
            double d21 = vp.Dot(edge1);

            double denomBary = d00 * d11 - d01 * d01;
            double v = (d11 * d20 - d01 * d21) / denomBary;
            double w = (d00 * d21 - d01 * d20) / denomBary;
            double u = 1.0 - v - w;

            // Interpolated normal (smooth shading)
            // TODO: ...
            Vector3 hitNormal = normal;
            if (n0.HasValue && n1.HasValue && n2.HasValue)
            {
                hitNormal = (u * n0.Value + v * n1.Value + w * n2.Value).Normalized();
            }

            //!----------------------------------------


            // Flip the normal if angle < 90
            // Vector3 hitNormal = normal;
            if (ray.Direction.Dot(hitNormal) > 0)
            {
                hitNormal = -hitNormal;
            }

            //!------------------------------------------
            // Texture coords
            // Interpolated UV
            TextureCoord? hitUV = null;
            if (uv0.HasValue && uv1.HasValue && uv2.HasValue)
            {
                double texU = u * uv0.Value.U + v * uv1.Value.U + w * uv2.Value.U;
                double texV = u * uv0.Value.V + v * uv1.Value.V + w * uv2.Value.V;
                hitUV = new TextureCoord(texU, texV);
            }
            //!------------------------------------------------------


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