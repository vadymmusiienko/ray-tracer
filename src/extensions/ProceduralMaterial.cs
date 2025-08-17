
using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a texture material that is associated with an entity
    /// that is to be rendered by the ray tracer. This material generates its
    /// texture procedurally based on defined patterns, such as stripes or
    /// checkers, rather than using an image file. 
    /// </summary>
    public class ProceduralMaterial : Material
    {
        public enum PatternType
        {
            Checkers,
            Stripes,
        }

        private PatternType pattern;
        private double scaleU;
        private double scaleV;
        private Color color1;
        private Color color2;

        /// <summary>
        /// Construct a new material object.
        /// </summary>
        /// <param name="pattern">The pattern of the material</param>
        /// <param name="scaleU">Scale factor in the U direction</param>
        /// <param name="scaleV">Scale factor in the V direction</param>
        /// <param name="color1">First color of the pattern</param>
        /// <param name="color2">Second color of the pattern</param>
        public ProceduralMaterial(PatternType pattern, double scaleU, double scaleV, Color color1, Color color2)
            : base(new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), 0, 0, 0, 1)
        {
            this.pattern = pattern;
            this.scaleU = scaleU;
            this.scaleV = scaleV;
            this.color1 = color1;
            this.color2 = color2;
        }

        /// <summary>
        /// Get the color of the material at a given texture coordinate.
        /// </summary>
        /// <param name="u">The u coordinate in the texture map</param>
        /// <param name="v">The v coordinate in the texture map</param>
        /// <returns>The color at the specified texture coordinate</returns>
        //!------------------------------
        //TODO: Finish this!
        public override Color GetDiffuseColor(TextureCoord? coord)
        {
            if (!coord.HasValue)
            {
                return this.color1;
            }

            // Scale texture coordinates to get how many times u and v repeats
            TextureCoord tex = coord.Value;
            double uRepeats = tex.U * scaleU;
            double vRepeats = tex.V * scaleV;

            // Checkers
            if (pattern == PatternType.Checkers)
            {
                // Convert double to an integer to alternate color
                // Use floor to map (0.0 - 0.99 to 0, 1.0 - 1.99 to 1, etc)
                int ui = (int)Math.Floor(uRepeats); // ! Should be in range (0, ScaleU)
                int vi = (int)Math.Floor(vRepeats); // Should be in range (0, ScaleV)

                // Alternate based on parity of ui vi sum
                if ((ui + vi) % 2 == 0)
                {
                    return this.color1;
                }
                else
                {
                    return this.color2;
                }

            }

            // Stripes
            if (pattern == PatternType.Stripes)
            {
                // Here we care only about u coordinate (alternate only 1 way to get stripes)
                // Use floor to map (0.0 - 0.99 to 0, 1.0 - 1.99 to 1, etc)
                int ui = (int)Math.Floor(uRepeats);

                // Alternate based on parity of ui
                if (ui % 2 == 0)
                {
                    return this.color1;
                }
                else
                {
                    return this.color2;
                }


            }

            // If a pattern is not implemented - return default color
            return this.color1;
        }
    }
}
