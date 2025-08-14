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

        public Ray GenerateRay(int px, int py)
        {
            // Find the mapping between px, py and image plane x and y
            double y = halfHeight - ((py + 0.5) / imageHeight * (halfHeight * 2f));
            double x = ((px + 0.5) / imageWidth * (halfWidth * 2)) - halfWidth;

            // Find ray's direction
            Vector3 dir = new Vector3(x, y, 1f).Normalized();

            // Cameras location is this ray's origin
            Vector3 origin = this.Transform.Position;

            // Return the ray
            return new Ray(origin, dir);
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
