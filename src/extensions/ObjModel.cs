using System;
using System.Collections.Generic;
using System.IO;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// !NOTE: I modified this class to only store obj object's faces
    /// !NOTE: Parcing happens in ObjReader
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
            // // Find the first hit (if any)
            // RayHit closestHit = null;
            // // Face closestFace = null;
            // double closestDistSq = double.MaxValue;

            // foreach (BVHTriangle face in this.faces)
            // {
            //     RayHit hit = face.Intersect(ray);

            //     if (hit != null)
            //     {
            //         double distSq = (hit.Position - ray.Origin).LengthSq();

            //         // Check if this object is closer to the camera
            //         if (distSq < closestDistSq)
            //         {
            //             closestDistSq = distSq;
            //             closestHit = hit;
            //             // closestFace = face;
            //         }
            //     }

            // }

            // // Return the closest hit
            // return closestHit;
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        // TODO: Do i need to do smth abt ProceduralMaterial here?
        public Material Material { get { return this.material; } }
    }
}
