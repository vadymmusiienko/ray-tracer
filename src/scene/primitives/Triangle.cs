using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
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

            // Inside of the triangle if reached this point (passed 3 tests)


            // Flip the normal if angle < 90
            Vector3 hitNormal = normal;
            if (ray.Direction.Dot(hitNormal) > 0)
            {
                hitNormal = -hitNormal;
            }

            // Find the incident
            Vector3 incident = -ray.Direction;

            // Create Rayhit object
            RayHit hit = new RayHit(hitPoint, hitNormal, incident, this.material);

            return hit;
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}
