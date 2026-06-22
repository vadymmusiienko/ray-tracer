namespace RayTracer
{
    /// <summary>
    /// Class to represent a texture material that is associated with an entity
    /// that is to be rendered by the ray tracer. This material uses an image
    /// as a texture map for the color and optionally a normal map for surface
    /// details. It extends the base Material class to include texture properties.
    /// </summary>
    public class TextureMaterial : Material
    {
        private Image colorMap;
        private Image normalMap;

        /// <summary>
        /// Construct a new material object.
        /// </summary>
        /// <param name="colorMap">The color map (texture) of the material</param>
        /// <param name="normalMap">The normal map of the material (optional)</param>
        public TextureMaterial(Image colorMap, Image normalMap = null)
            : base(new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), 0, 0, 0, 1)
        {
            this.colorMap = colorMap;
            this.normalMap = normalMap;
        }

        public override Color GetDiffuseColor(TextureCoord? coord)
        {
            if (!coord.HasValue)
            {
                return new Color(0, 0, 0); // Fall black if no coords
            }

            int x = (int)(coord.Value.U * (colorMap.Width - 1));
            int y = (int)((1 - coord.Value.V) * (colorMap.Height - 1)); // flip V if needed

            return colorMap.GetPixel(x, y);
        }
    }
}
