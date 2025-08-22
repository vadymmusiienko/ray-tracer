using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent BVH
    /// </summary>
    public class BVH
    {
        // Max number of splits
        const int MaxDepth = 20; // ! Can be changed

        // Min number of triangles in a node
        const int MinTriPerNode = 4; // ! Can be changed

        // Store the root node
        Node root;

        // Constructor for the BVH
        public BVH
        (
            List<Vector3> vertices, List<Vector3> normals, List<TextureCoord> textureCoords,
            List<int> vIndices, List<int> vtIndices, List<int> vnIndices, Material material
        )
        {

            // Create the bounding box
            BoundingBox bounds = new BoundingBox();

            foreach (Vector3 vert in vertices)
            {
                bounds.GrowToInclude(vert);
            }

            // Create triangles
            int triangleCount = vIndices.Count / 3;
            BVHTriangle[] triangles = new BVHTriangle[triangleCount];

            for (int i = 0; i < triangleCount; i++)
            {
                // Handle negative indices by using GetIndex helper method
                Vector3 v0 = vertices[GetIndex(vIndices[i * 3 + 0], vertices.Count)];
                Vector3 v1 = vertices[GetIndex(vIndices[i * 3 + 1], vertices.Count)];
                Vector3 v2 = vertices[GetIndex(vIndices[i * 3 + 2], vertices.Count)];

                TextureCoord? uv0 = (vtIndices.Count > i * 3 + 0) ?
                    textureCoords[GetIndex(vtIndices[i * 3 + 0], textureCoords.Count)] : null;
                TextureCoord? uv1 = (vtIndices.Count > i * 3 + 1) ?
                    textureCoords[GetIndex(vtIndices[i * 3 + 1], textureCoords.Count)] : null;
                TextureCoord? uv2 = (vtIndices.Count > i * 3 + 2) ?
                    textureCoords[GetIndex(vtIndices[i * 3 + 2], textureCoords.Count)] : null;

                Vector3? n0 = (vnIndices.Count > i * 3 + 0) ?
                    normals[GetIndex(vnIndices[i * 3 + 0], normals.Count)] : null;
                Vector3? n1 = (vnIndices.Count > i * 3 + 1) ?
                    normals[GetIndex(vnIndices[i * 3 + 1], normals.Count)] : null;
                Vector3? n2 = (vnIndices.Count > i * 3 + 2) ?
                    normals[GetIndex(vnIndices[i * 3 + 2], normals.Count)] : null;


                // Create triangle
                triangles[i] = new BVHTriangle(v0, v1, v2, uv0, uv1, uv2, n0, n1, n2, material);
            }

            // Create root node
            root = new Node() { bounds = bounds, triangles = new List<BVHTriangle>(triangles) };
            Split(root);
        }

        // Helper function to deal with negative indices
        private int GetIndex(int objIndex, int listCount)
        {
            if (objIndex >= 0)
                return objIndex;
            else
                return listCount + objIndex + 1; // Negative index: relative to end
        }

        // Helper function that splits the bounding box into 2 parts A and B
        static void Split(Node parent, int depth = 0)
        {
            // Reached the last split - base case
            if (depth == MaxDepth)
            {
                return;
            }

            // Slplit by the longest axis
            Vector3 size = parent.bounds.size;
            // 0 - X-axis, 1 - Y-axis, 2 - Z-axis
            int SplitAxis = size.X > Math.Max(size.Y, size.Z) ? 0 : size.Y > size.Z ? 1 : 2;
            Vector3 center = parent.bounds.center;
            double SplitPos = SplitAxis == 0 ? center.X : SplitAxis == 1 ? center.Y : center.Z;


            // Create children nodes and assing triangles to them
            parent.childA = new Node();
            parent.childB = new Node();

            foreach (BVHTriangle tri in parent.triangles)
            {
                Vector3 triCenter = tri.Center;
                bool inA = SplitAxis == 0 ? triCenter.X < SplitPos :
                           SplitAxis == 1 ? triCenter.Y < SplitPos :
                           triCenter.Z < SplitPos;

                Node child = inA ? parent.childA : parent.childB;
                child.triangles.Add(tri);
                child.bounds.GrowToInclude(tri);
            }

            // Recursively split it's children
            if (parent.childA.TriangleCount > MinTriPerNode && parent.childB.TriangleCount > MinTriPerNode)
            {
                // Clear parent triangles to save memory
                parent.triangles.Clear();
                Split(parent.childA, depth + 1);
                Split(parent.childB, depth + 1);
            }
        }

        // Function to find the intersection point if any
        public RayHit Intersect(Ray ray)
        {
            if (root == null)
            {
                return null;
            }

            return IntersectNode(ray, root);

        }

        // Recursive helper method to intersect with a node
        private RayHit IntersectNode(Ray ray, Node node)
        {
            // First check if intersects the bounding box
            if (!IntersectBoundingBox(ray, node.bounds))
            {
                return null;
            }

            RayHit closestHit = null;
            double closestDistanceSq = double.MaxValue;

            // If this is a leaf node - check all triangles
            if (node.childA == null && node.childB == null)
            {
                foreach (BVHTriangle tri in node.triangles)
                {
                    RayHit hit = tri.Intersect(ray);

                    // Hit!
                    if (hit != null)
                    {
                        double distSq = (hit.Position - ray.Origin).LengthSq();

                        // Closest hit
                        if (distSq < closestDistanceSq)
                        {
                            closestHit = hit;
                            closestDistanceSq = distSq;
                        }
                    }
                }
            }
            else
            {
                // Internal node
                // Recurse into child A
                if (node.childA != null)
                {
                    RayHit hitA = IntersectNode(ray, node.childA);
                    if (hitA != null)
                    {
                        double distSq = (hitA.Position - ray.Origin).LengthSq();

                        // Closest hit
                        if (distSq < closestDistanceSq)
                        {
                            closestHit = hitA;
                            closestDistanceSq = distSq;
                        }
                    }
                }

                // Recurse into child B
                if (node.childB != null)
                {
                    RayHit hitB = IntersectNode(ray, node.childB);
                    if (hitB != null)
                    {
                        double distSq = (hitB.Position - ray.Origin).LengthSq();

                        // Closest hit
                        if (distSq < closestDistanceSq)
                        {
                            closestHit = hitB;
                            closestDistanceSq = distSq;
                        }
                    }
                }
            }

            return closestHit;
        }

        // Helper method to find if ray intersects the bounding box (Slab method)
        private bool IntersectBoundingBox(Ray ray, BoundingBox box)
        {
            // X-axis slab
            double tx1 = (box.min.X - ray.Origin.X) / ray.Direction.X;
            double tx2 = (box.max.X - ray.Origin.X) / ray.Direction.X;
            double tmin = Math.Min(tx1, tx2);
            double tmax = Math.Max(tx1, tx2);

            // Y-axis slab
            double ty1 = (box.min.Y - ray.Origin.Y) / ray.Direction.Y;
            double ty2 = (box.max.Y - ray.Origin.Y) / ray.Direction.Y;
            tmin = Math.Max(tmin, Math.Min(ty1, ty2));
            tmax = Math.Min(tmax, Math.Max(ty1, ty2));

            // Z-axis slab
            double tz1 = (box.min.Z - ray.Origin.Z) / ray.Direction.Z;
            double tz2 = (box.max.Z - ray.Origin.Z) / ray.Direction.Z;
            tmin = Math.Max(tmin, Math.Min(tz1, tz2));
            tmax = Math.Min(tmax, Math.Max(tz1, tz2));

            return tmax >= tmin && tmax > 0;
        }

        // Helper Inner class for a BoundingBox that can grow given a triangle
        private class BoundingBox
        {
            public Vector3 min = Vector3.One * double.PositiveInfinity;
            public Vector3 max = Vector3.One * double.NegativeInfinity;
            public Vector3 center => (min + max) * 0.5;
            public Vector3 size => max - min;

            public void GrowToInclude(Vector3 point)
            {
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }

            public void GrowToInclude(BVHTriangle triangle)
            {
                GrowToInclude(triangle.V0);
                GrowToInclude(triangle.V1);
                GrowToInclude(triangle.V2);
            }
        }

        // Helper Inner class for representing a node with a BoundingBox and 2 children
        private class Node
        {
            public BoundingBox bounds = new BoundingBox();
            public List<BVHTriangle> triangles = new List<BVHTriangle>();
            public int TriangleCount => triangles.Count;
            public Node childA;
            public Node childB;
        }

    }

}