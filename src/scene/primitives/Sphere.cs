using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            Vector3 L = ray.Origin - this.center; // This shifts the sphere to (0, 0)

            // Find a, b, c of quadratic equation (for discriminant)
            // TODO: Is a always zero, because its normalized?
            //double a = ray.Direction.Dot(ray.Direction); // Ray.direction^2
            double b = 2 * L.Dot(ray.Direction); // 2 Ray.origin * Ray.direction
            double c = L.LengthSq() - radius * radius; // Ray.origin^2 - r^2

            // Solve quadratic equation
            // TODO: add 4 * a if doesn't work
            double discr = b * b - 4 * c;

            // No solutions check
            if (discr < 0)
            {
                return null;
            }

            // This is final distance (for the first hit, we don't care about the second)
            double t;

            // One solution (discr is 0, 1e-6f is basically 0)
            if (Math.Abs(discr) < 1e-6f)
            {
                // TODO: Add (2 * a) in the denom if doesn't work
                t = -b / 2;

                // The hit is behind the ray
                if (t < 0)
                {
                    return null;
                }
            }
            else
            {
                // Two solutions
                // TODO: Add (2 * a) in the denom for t1 and t2 if doesn't work
                double sqrt_discr = Math.Sqrt(discr);
                double t1 = (-b - sqrt_discr) / 2;
                double t2 = (-b + sqrt_discr) / 2;

                // Pick the smallest positive t
                t = Math.Min(t1, t2);
                if (t < 0f) t = Math.Max(t1, t2);
                if (t < 0f) return null; // both behind the ray
            }
            // Find the hitpoint
            Vector3 hitPoint = ray.Origin + ray.Direction * t;

            // Find normal to the hitpoint for a sphere
            // TODO: Instead of using .Normalized(), can divide by the radius - might be faster
            Vector3 normal = (hitPoint - this.center).Normalized();

            // TODO: Does incident need to be inverted?
            Vector3 incident = -ray.Direction;

            // Return the hit
            RayHit hit = new RayHit(hitPoint, normal, incident, this.material);

            return hit;
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
