using System;
using System.Numerics;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Plane : SceneEntity
    {
        private Vector3 center;
        private Vector3 normal;
        private Material material;

        /// <summary>
        /// Construct an infinite plane object.
        /// </summary>
        /// <param name="center">Position of the center of the plane</param>
        /// <param name="normal">Direction that the plane faces</param>
        /// <param name="material">Material assigned to the plane</param>
        public Plane(Vector3 center, Vector3 normal, Material material)
        {
            this.center = center;
            this.normal = normal.Normalized();
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the plane, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
        
        // Denom
        double denom = ray.Direction.Dot(this.normal);

        // Parallel - doens't intersect (1e-6f is basically 0)
        if (Math.Abs(denom) < 1e-6f)
        {
            return null;
        }

        // Nom
        double nom = this.normal.Dot(this.center - ray.Origin);

        // t - distance from ray's origin to the hit point
        double t = nom / denom;

        // The hit point is behind ray origin
        if (t < 0)
        {
            return null;
        }

        // Find the hitpoint
        Vector3 hitPoint = ray.Origin + ray.Direction * t;

        // TODO: ??? Do I need to flip normal if angle < 90?
        // Vector3 hitNormal = this.normal;
        //     if (Vector3.Dot(ray.Direction, hitNormal) > 0)
        //     {
        //         hitNormal = -hitNormal;
        //     }

        // TODO: Does incident need to be inverted?
        Vector3 incident = -ray.Direction;

        // Create Rayhit object
        RayHit hit = new RayHit(hitPoint, this.normal, incident, this.material);

        return hit;
        }

        /// <summary>
        /// The material of the plane.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
