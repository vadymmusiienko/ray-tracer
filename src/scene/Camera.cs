using System;

namespace RayTracer
{
    /// <summary>
    /// Represents a camera in the ray tracing scene.
    /// </summary>
    public class Camera : SceneEntity
    {
        // Store current half width and half height of the image plane
        private double halfWidth;
        private double halfHeight;

        // Store image width and image height
        private int imageWidth;
        private int imageHeight;
        // Random number generator for aperture sampling
        private static Random random = new Random();

        public Transform Transform { get; private set; }

        /// <summary>
        /// Construct a new camera with a specified transformation.
        /// </summary>
        /// <param name="transform">Transformation to apply to the camera</param>
        public Camera(Transform transform)
        {
            this.Transform = transform;
        }

        /// <summary>
        /// Compute Image World Boundaries and set width and height
        /// </summary>
        /// <param name="FOV">Horizontal FOV</param>
        /// <param name="aspectRatio">Aspect ratio of the image (width / height)</param>
        public void ComputeWorldImageBounds(double FOV, int imageWidth, int imageHeight)
        {
            // Save image width and height
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;

            // Find image plane half width and half height
            double aspectRatio = (double)imageWidth / (double)imageHeight;
            halfWidth = Math.Tan(FOV * 0.5 * Math.PI / 180.0);
            halfHeight = halfWidth / aspectRatio;
        }

        public Ray GenerateRay(int px, int py, double offsetX, double offsetY, double apertureRadius, double focalLength)
        {
            // Find the mapping between px, py and image plane x and y
            double y = halfHeight - ((py + offsetY) / imageHeight * (halfHeight * 2f));
            double x = ((px + offsetX) / imageWidth * (halfWidth * 2)) - halfWidth;
            Vector3 dir = new Vector3(x, y, 1.0).Normalized();

            // Apply the camera rotation
            dir = this.Transform.Rotation.Rotate(dir);

            // Find ray's origin (camera's position)
            Vector3 origin = this.Transform.Position;

            // ! Depth of field blur
            if (apertureRadius > 0.0)
            {
                // Calculate the focal point
                Vector3 focalPoint = origin + (dir * focalLength);

                // Find a random point on aperture (z = 0)
                Vector3 aperturePoint = sampleAperture(apertureRadius);

                // Rotate with the camera to find the actual offset
                Vector3 apertureOffset = this.Transform.Rotation.Rotate(aperturePoint);

                // Update the ray origin by adding the offset
                origin += apertureOffset; // This is a new random point on camera's aperture

                // Find the new direction from this new origin to the focal point
                dir = (focalPoint - origin).Normalized();

            }

            // Works for both pinhole camera and camera with depth of field blur
            return new Ray(origin, dir);
        }

        /// <summary>
        /// Helper static method to generate a random point in a circle (aperture)
        /// Used for depth of field blur
        /// </summary>
        public static Vector3 sampleAperture(double radius)
        {
            // Generate uniform distribution within circle using polar coordinates
            double r = radius * Math.Sqrt(random.NextDouble()); // Square root for uniform area distribution
            double theta = random.NextDouble() * 2.0 * Math.PI;

            // Convert to Cartesian coordinates (centered at origin)
            double x = r * Math.Cos(theta);
            double y = r * Math.Sin(theta);

            return new Vector3(x, y, 0);
        }

        /// <summary>
        /// Cameras do not intersect with rays in the same way as other entities.
        /// This method can is left unimplemented.
        /// </summary>
        public RayHit Intersect(Ray ray)
        {
            throw new NotImplementedException("Camera does not support intersection checks.");
        }

        /// <summary>
        /// Cameras do not have a material in the same way as other entities.
        /// This property is left unimplemented.
        /// </summary>
        public Material Material
        {
            get { throw new NotImplementedException("Camera does not have a material."); }
        }
    }
}
