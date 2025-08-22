using System;

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
        private double textureScale;

        // Basis vectors for texture mapping
        private Vector3 uAxis;
        private Vector3 vAxis;

        /// <summary>
        /// Construct an infinite plane object.
        /// </summary>
        /// <param name="center">Position of the center of the plane</param>
        /// <param name="normal">Direction that the plane faces</param>
        /// <param name="material">Material assigned to the plane</param>
        /// <param name="textureScale">Scale factor for texture mapping (default: 1.0)</param>
        public Plane(Vector3 center, Vector3 normal, Material material, double textureScale = 1.0)
        {
            this.center = center;
            this.normal = normal.Normalized();
            this.material = material;
            this.textureScale = textureScale;

            // Create orthogonal basis vectors for texture mapping
            CreateTextureBasis();
        }

        /// <summary>
        /// Create orthogonal basis vectors for texture coordinate mapping
        /// </summary>
        private void CreateTextureBasis()
        {
            // Choose an arbitrary vector that's not parallel to the normal
            Vector3 arbitrary = Math.Abs(this.normal.X) < 0.9 ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);

            // Create two orthogonal vectors in the plane
            this.uAxis = this.normal.Cross(arbitrary).Normalized();
            this.vAxis = this.normal.Cross(this.uAxis).Normalized();
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

            // Parallel - doesn't intersect (1e-6 is basically 0)
            if (Math.Abs(denom) < 1e-6)
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

            // Calculate texture coordinates
            Vector3 localPoint = hitPoint - this.center;
            double texU = localPoint.Dot(this.uAxis) / this.textureScale;
            double texV = localPoint.Dot(this.vAxis) / this.textureScale;

            // Wrap texture coordinates to [0,1] range
            texU = texU - Math.Floor(texU);
            texV = texV - Math.Floor(texV);

            TextureCoord hitUV = new TextureCoord(texU, texV);

            // Flip normal if angle < 90
            Vector3 hitNormal = this.normal;
            if (ray.Direction.Dot(hitNormal) > 0)
            {
                hitNormal = -hitNormal;
            }

            // Find the incident
            Vector3 incident = -ray.Direction;

            // Create RayHit object with texture coordinates
            RayHit hit = new RayHit(hitPoint, hitNormal, incident, this.material, hitUV);

            return hit;
        }

        /// <summary>
        /// The material of the plane.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}