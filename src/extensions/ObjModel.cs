namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// This stores the BVH Tree with all the triangles for optimization
    /// </summary>
    public class ObjModel : SceneEntity
    {
        // Store the BVH Tree representing the object
        private BVH bvhTree;
        private Material material;

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="transform">Transform to apply to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Transform transform, Material material)
        {
            this.material = material;

            // Call the helper class to actually parse the file
            bvhTree = ObjReader.Parse(objFilePath, transform, material);
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            return bvhTree.Intersect(ray);
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}
