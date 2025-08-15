using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private Camera camera;
        private Color ambientLightColor;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;
        private ISet<Animation> animations;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.camera = new Camera(Transform.Identity);
            this.ambientLightColor = new Color(0, 0, 0);
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
            this.animations = new HashSet<Animation>();
        }

        /// <summary>
        /// Set the camera for the scene.
        /// </summary>
        /// <param name="camera">Camera object</param>
        public void SetCamera(Camera camera)
        {
            this.camera = camera;
        }

        /// <summary>
        /// Set the ambient light color for the scene.
        /// </summary>
        /// <param name="color">Color object</param>
        public void SetAmbientLightColor(Color color)
        {
            this.ambientLightColor = color;
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Add an animation to the scene.
        /// </summary>
        /// <param name="animation">Animation object</param>
        public void AddAnimation(Animation animation)
        {
            this.animations.Add(animation);
        }

        /// <summary>
        /// Calcualte Diffuse Reflection for a RayHit (overall light - makes it 3d)
        /// <param name="rayHit">RayHit object</param>
        /// <param name="Cmd">Material’s diffuse color</param>
        /// TODO: Move this method somewehre else (like PointLight class?)
        public Color DiffuseReflection(RayHit rayHit, Color Cmd)
        {
            // Cd = Cmd Cl max(0, ˆN · ˆL) - Lambertian model
            Color finalDiffuse = new Color(0, 0, 0);

            foreach (PointLight light in this.lights)
            {
                // ˆN - Surface normal
                Vector3 N = rayHit.Normal;

                // ˆL - Direction from the hit point to the light
                Vector3 L = (light.Position - rayHit.Position).Normalized();

                // Cl - Light color
                Color Cl = light.Color;

                // Cd - Diffuse reflection component
                Color Cd = Cmd * Cl * Math.Max(0, N.Dot(L));

                // Add to the final diffuse
                finalDiffuse += Cd;
            }

            return finalDiffuse;
        }

        /// <summary>
        /// Calcualte Specular Reflection for a RayHit (shiny highlights)
        /// <param name="rayHit">RayHit object</param>
        /// <param name="Cms">Material’s specular color</param>
        /// <param name="Ns">Shininess exponent (controls sharpness of the highlight)</param>
        /// TODO: Move this method somewehre else (like PointLight class?)
        public Color SpecularReflection(RayHit rayHit, Color Cms, double Ns)
        {
            // Cs = Cms Cl [max(0, ˆR · ˆV )]^Ns
            Color finalSpecular = new Color(0, 0, 0);

            foreach (PointLight light in this.lights)
            {
                // ˆN - Surface normal
                Vector3 N = rayHit.Normal;

                // ˆL - Direction from the hit point to the light
                Vector3 L = (light.Position - rayHit.Position).Normalized();

                // Cl - Light color
                Color Cl = light.Color;

                // ˆV - Direction towards the camera
                Vector3 V = (camera.Transform.Position - rayHit.Position).Normalized();

                // ˆR - Mirror reflection direction of the light
                Vector3 R = (2 * N.Dot(L) * N - L).Normalized(); // TODO: Check this formula

                // Cs - Specular reflection component
                double factor = Math.Max(0, R.Dot(V));

                // If factor is 0, then Cs is 0
                if (factor == 0)
                {
                    // This if statement fixes float arithmetic inaccuracy
                    continue;
                }

                // Cs - Specular reflection component
                Color Cs = Cms * Cl * Math.Pow(factor, Ns);

                // Add to the final specular
                finalSpecular += Cs;
            }

            return finalSpecular;
        }

        /// <summary>
        /// Calcualte Ambient Reflection for a RayHit
        /// <param name="Cma">Material’s ambient color<param\>
        /// TODO: Move this method somewehre else (like PointLight class?)
        private Color AmbientReflection(Color Cma)
        {
            // Ca = Cma Cla
            // ambientLightColor is Cla
            Color Ca = Cma * ambientLightColor;
            return Ca;
        }

        /// <summary>
        /// Calcualte the final local colour a RayHit
        /// <param name="rayHit">RayHit object</param>
        /// <param name="mat">Hit object's material</param>
        /// TODO: Move this method somewehre else (like PointLight class?) 
        public Color PhongShading(RayHit rayHit, Material mat)
        {
            Color Ca = AmbientReflection(mat.AmbientColor);
            Color CdSum = DiffuseReflection(rayHit, mat.DiffuseColor);
            Color CsSum = SpecularReflection(rayHit, mat.SpecularColor, mat.Shininess);

            Color Clocal = Ca + CdSum + CsSum;
            return Clocal;
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        /// <param name="time">Time since start in seconds</param>
        public void Render(Image outputImage, double time = 0)
        {
            // Set world image boundaries
            camera.ComputeWorldImageBounds(60.0, outputImage.Width, outputImage.Height);

            // Fire rays into the world
            for (int py = 0; py < outputImage.Height; py++)
            {
                for (int px = 0; px < outputImage.Width; px++)
                {
                    // Fire a ray through this pixel
                    Ray ray = camera.GenerateRay(px, py);

                    // See if the ray hit anything
                    foreach (SceneEntity entity in this.entities)
                    {
                        RayHit hit = entity.Intersect(ray);
                        if (hit != null)
                        {
                            // We got a hit!
                            // TODO: Make sure to check if this is the first hit or not
                            // TODO: Save the Rayhit in a set or smth
                            //Color color = entity.Material.DiffuseColor;
                            //outputImage.SetPixel(px, py, color);

                            // Use Phong Shading to find the color of this pixel
                            Color finalColor = PhongShading(hit, entity.Material);

                            // Set this pixel's color
                            outputImage.SetPixel(px, py, finalColor);
                        }
                    }
                }
            }

        }
    }
}
