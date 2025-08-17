using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent ray hit data, including the position and
    /// normal of a hit (and optionally other computed vectors).
    /// </summary>
    public class RayHit
    {
        private Vector3 position;
        private Vector3 normal;
        private Vector3 incident;

        // !NOTE: Carry Texture coordinates if present
        private TextureCoord? textureCoord;

        public RayHit(Vector3 position, Vector3 normal, Vector3 incident, Material material)
        {
            this.position = position;
            this.normal = normal;
            this.incident = incident;
        }

        // RayHit constructor for objects with textures
        public RayHit(Vector3 position, Vector3 normal, Vector3 incident, Material material, TextureCoord? textureCoord)
        {
            this.position = position;
            this.normal = normal;
            this.incident = incident;
            this.textureCoord = textureCoord;
        }

        // You may wish to write methods to compute other vectors, 
        // e.g. reflection, transmission, etc

        // Return texture coordinates if present
        public TextureCoord? TextureCoord { get { return this.textureCoord; } }
        public Vector3 Position { get { return this.position; } }

        public Vector3 Normal { get { return this.normal; } }

        public Vector3 Incident { get { return this.incident; } }
    }
}
