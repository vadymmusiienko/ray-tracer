using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        // Max depth for reflection recursion
        private const int MaxDepth = 5; // TODO: Move/Change?

        // Samples per pixel for Depth of field blur
        private const int samplesPerPixDOF = 10; // TODO: Change this number

        // Horizontal FOV
        private const double FOV = 60.0;

        // Default black color
        private Color black = new Color(0, 0, 0); // TODO: Move/change?
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
            // Ambient fill light. When enabled, a small fraction of each
            // surface's own colour is added everywhere, so areas that receive no
            // direct light (shadows, cavities) read as a dim tint of the surface
            // instead of pure black. Disabled => 0, preserving the original look.
            if (this.options.AmbientLightingEnabled)
            {
                this.ambientLightColor = new Color(0.18, 0.18, 0.18);
            }
            else
            {
                this.ambientLightColor = new Color(0, 0, 0);
            }
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
                // How much of this light reaches the point (0 = shadowed,
                // 1 = clear; partial when behind glass).
                double vis = ShadowVisibility(rayHit.Position, light.Position);
                if (vis <= 0.0)
                {
                    continue;
                }

                // ˆN - Surface normal, oriented to face the viewer so back
                // faces (e.g. the inside of a sphere) are lit instead of black.
                Vector3 N = rayHit.Normal;
                if (N.Dot(rayHit.Incident) < 0) N = -N;

                // ˆL - Direction from the hit point to the light
                Vector3 L = (light.Position - rayHit.Position).Normalized();

                // Cl - Light color
                Color Cl = light.Color;

                // Cd - Diffuse reflection component
                Color Cd = Cmd * Cl * Math.Max(0, N.Dot(L)) * vis;

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
                // How much of this light reaches the point (0 = shadowed).
                double vis = ShadowVisibility(rayHit.Position, light.Position);
                if (vis <= 0.0)
                {
                    continue;
                }

                // ˆN - Surface normal, oriented to face the viewer (two-sided).
                Vector3 N = rayHit.Normal;
                if (N.Dot(rayHit.Incident) < 0) N = -N;

                // ˆL - Direction from the hit point to the light
                Vector3 L = (light.Position - rayHit.Position).Normalized();

                // Cl - Light color
                Color Cl = light.Color;

                // ˆV - Direction towards the camera
                Vector3 V = (camera.Transform.Position - rayHit.Position).Normalized();

                // ˆR - Mirror reflection direction of the light
                Vector3 R = (2 * N.Dot(L) * N - L).Normalized();

                // Cs - Specular reflection component
                double factor = Math.Max(0, R.Dot(V));

                // If factor is 0, then Cs is 0
                if (factor == 0)
                {
                    // This if statement fixes float arithmetic inaccuracy
                    continue;
                }

                // Cs - Specular reflection component
                Color Cs = Cms * Cl * Math.Pow(factor, Ns) * vis;

                // Add to the final specular
                finalSpecular += Cs;
            }

            return finalSpecular;
        }

        /// <summary>
        /// Calcualte Ambient Reflection for a RayHit
        /// <param name="Cma">Material’s ambient color<param\>
        /// TODO: Move this method somewehre else (like PointLight class?)
        /// <summary>
        /// Calcualte the final local colour a RayHit
        /// <param name="rayHit">RayHit object</param>
        /// <param name="mat">Hit object's material</param>
        /// TODO: Move this method somewehre else (like PointLight class?) 
        public Color PhongShading(RayHit rayHit, Material mat)
        {
            // In order to account for textures, find diffuse color at a specific point
            //!---------------------------------------------
            // If has a texture - gets diffuse color at the texture coord - otherwise just base color
            Color diffuseBase = mat.GetDiffuseColor(rayHit.TextureCoord);

            // Ambient/fill term. Materials carry no ambient colour of their own, so tie the
            // ambient contribution to the surface's own diffuse colour. With ambient lighting
            // enabled this keeps fully shadowed opaque surfaces a dim tint instead of jet black.
            Color Ca = ambientLightColor * diffuseBase;

            Color CdSum = DiffuseReflection(rayHit, diffuseBase);
            Color CsSum = SpecularReflection(rayHit, mat.SpecularColor, mat.Shininess);

            Color Clocal = Ca + CdSum + CsSum;
            return Clocal;
        }

        /// <summary>
        /// A helper method to check if the pixel is in shadow
        /// <param name="hitPosition">RayHit's position</param>
        /// <param name="lightPosition">Light source's position</param>
        /// TODO: Move this method somewehre else (like PointLight class?) 
        private double ShadowVisibility(Vector3 hitPosition, Vector3 lightPosition)
        {
            // Shadow ray's direction (Direction to light)
            Vector3 rayDir = (lightPosition - hitPosition).Normalized();

            // Shadow ray's origin position slightly shifted (to avoid a 'premature' hit)
            Vector3 rayOrigin = hitPosition + rayDir * 1e-6;

            // Shadow ray (from the hit object towards the light source)
            Ray shadowRay = new Ray(rayOrigin, rayDir);

            // The distance from the hit position to the light source (Sq is faster to compute)
            double distanceToLightSq = (lightPosition - rayOrigin).LengthSq();

            // How much of the light reaches the point: 1 = fully lit, 0 = fully
            // shadowed. Opaque occluders block completely; transmissive ones
            // (glass) let a fraction through, so they cast a soft, tinted shadow
            // instead of a jet-black one.
            double visibility = 1.0;
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(shadowRay);
                if (hit == null) continue;

                // Ignore occluders beyond the light.
                double hitDistanceSq = (hit.Position - rayOrigin).LengthSq();
                if (hitDistanceSq >= distanceToLightSq) continue;

                double kt = entity.Material.Transmissivity;
                if (kt <= 0.0) return 0.0; // opaque: fully shadowed, stop early
                visibility *= kt;          // transparent: attenuate and keep going
            }
            return visibility;
        }

        /// <summary>
        /// 1. See if the ray hits any objects
        /// 2. Find the closest hit
        /// 3. Find the color of the pixel at this hit
        /// 4. Reflect if reflective
        /// 5. Return final color or null if no hit
        /// </summary>
        /// <param name="ray">Ray to trace output</param>
        /// <param name="currDepth">Current depth of the recursion (for reflective objects only)</param>
        public Color TraceRay(Ray ray, int currDepth = 0)
        {
            // Check if current depth exceeds max depth
            if (currDepth > MaxDepth)
            {
                return this.black; // No more contributions
            }

            // Find the first hit (if any)
            RayHit closestHit = null;
            SceneEntity closestEntity = null;
            double closestDistSq = double.MaxValue;

            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);

                if (hit != null)
                {
                    double distSq = (hit.Position - ray.Origin).LengthSq();

                    // Check if this object is closer to the camera
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closestHit = hit;
                        closestEntity = entity;
                    }
                }

            }

            // Check if hit anything
            if (closestHit == null)
            {
                return this.black; // No hit
            }

            // Get color using Phong shading method
            Color currColor = PhongShading(closestHit, closestEntity.Material);

            // Ray direction and Rayhit normal for reflection and refraction
            Vector3 D = ray.Direction;
            Vector3 N = closestHit.Normal;

            // Reflection
            double Kr = closestEntity.Material.Reflectivity; // Reflectivity
            Color reflectionColor = this.black;
            if (Kr > 0)
            {
                // Reflect about a normal that faces the incoming ray, so the
                // ray reflects correctly whether hitting a front or back face.
                Vector3 Nf = D.Dot(N) > 0 ? -N : N;

                // C_reflected = Kr · Trace(R_reflected, depth + 1)
                // Compute the reflection ray : R = D − 2 (D · N)N
                Vector3 R = (D - 2 * D.Dot(Nf) * Nf).Normalized(); // Reflection ray direction

                // Offset to avoid "premature" hit (self-intersection acne)
                Vector3 reflectionOrigin = closestHit.Position + R * 1e-4;

                Ray reflectionRay = new Ray(reflectionOrigin, R); // Reflection ray

                // Fire the reflection ray (with new reflection depth)
                reflectionColor = TraceRay(reflectionRay, currDepth + 1);
            }

            // Refraction
            double Kt = closestEntity.Material.Transmissivity;
            Color refractionColor = this.black;
            if (Kt > 0)
            {
                // C_refracted = Kt · Trace(R_refracted, depth + 1)
                // Compute the refraction ray
                double cos_i = -D.Dot(N);
                Vector3 normal = N; // Copy the normal (in case we flip it)
                double n_i = 1.0; // TODO: Air = 1.0?
                double n_t = closestEntity.Material.RefractiveIndex;

                // Exiting the material
                if (cos_i < 0)
                {
                    // Flip cos and normal
                    cos_i = -cos_i;
                    normal = -normal;

                    // Flip n_i and n_t
                    var temp = n_i;
                    n_i = n_t;
                    n_t = temp;
                }

                double n = n_i / n_t;
                double under_sqrt = 1 - n * n * (1 - cos_i * cos_i);

                // Cannot take sqrt of neg nums
                if (under_sqrt >= 0)
                {
                    // Refraction ray direction
                    Vector3 T = (n * D + ((n * cos_i - Math.Sqrt(under_sqrt))) * normal).Normalized();

                    // Offset to avoid "premature" hit
                    Vector3 refractionOrigin = closestHit.Position + T * 1e-4;

                    Ray refractionRay = new Ray(refractionOrigin, T); // Refraction ray

                    // Fire the refraction ray (with new reflection depth)
                    refractionColor = TraceRay(refractionRay, currDepth + 1);
                }
                else
                {
                    // Total internal reflection: the angle exceeds the critical
                    // angle so no ray transmits. Reflect internally instead of
                    // returning black (this is what mirrors the inside of a
                    // glass pyramid / the silhouette of a glass sphere).
                    Vector3 Rtir = (D - 2 * D.Dot(normal) * normal).Normalized();
                    Vector3 tirOrigin = closestHit.Position + Rtir * 1e-4;
                    refractionColor = TraceRay(new Ray(tirOrigin, Rtir), currDepth + 1);
                }

            }

            // Combine with additive blending: the local shading is kept and the
            // reflected and refracted contributions are ADDED on top (scaled by
            // their coefficients), rather than attenuating the local colour by
            // (1 - Kr - Kt). Additive blending is what lets a surface that is
            // both reflective and refractive show both effects together.
            currColor += reflectionColor * Kr; // Add the reflected contribution
            currColor += refractionColor * Kt; // Add the refracted contribution

            return currColor;

        }

        /// <summary>
        /// Render the scene to an output image. The pixel loop is parallelized
        /// across rows: each pixel writes a distinct region of the output buffer
        /// and only reads immutable scene state, so the work is independent and
        /// safe to run concurrently (DOF sampling uses the thread-safe
        /// Random.Shared). This is the single biggest performance win over the
        /// original single-threaded loop.
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        /// <param name="time">Time since start in seconds (for animations)</param>
        /// <param name="progress">Optional per-row progress callback (0..1)</param>
        /// <param name="cancellationToken">Cancels an in-flight render</param>
        /// <param name="maxDegreeOfParallelism">Max worker threads (-1 = unlimited)</param>
        public void Render(
            Image outputImage,
            double time = 0,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default,
            int maxDegreeOfParallelism = -1)
        {
            // Set world image boundaries (once, before the parallel region).
            camera.ComputeWorldImageBounds(FOV, outputImage.Width, outputImage.Height);

            // Get current AA settings (Anti-aliasing)
            int AAMult = this.options.AAMultiplier;

            // Get current Depth of field blur settings
            double apertureRadius = this.options.ApertureRadius; // Default 0 - No field of blur
            double focalLength = this.options.FocalLength; // Default 1

            // Determine if DOF is enabled (1 sample per pixel if not)
            int samplesDOF = apertureRadius > 0.0 ? samplesPerPixDOF : 1;

            int width = outputImage.Width;
            int height = outputImage.Height;
            int rowsCompleted = 0;

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            };

            // Fire rays into the world, one row per parallel work item.
            Parallel.For(0, height, parallelOptions, py =>
            {
                for (int px = 0; px < width; px++)
                {
                    RenderPixel(px, py, AAMult, samplesDOF, apertureRadius, focalLength, outputImage);
                }

                if (progress != null)
                {
                    int done = Interlocked.Increment(ref rowsCompleted);
                    progress.Report((double)done / height);
                }
            });
        }

        /// <summary>
        /// Render a single pixel, accumulating anti-aliasing and depth-of-field
        /// samples. Pure with respect to scene state except for the single
        /// write to the pixel's own location in the output image.
        /// </summary>
        private void RenderPixel(
            int px, int py, int AAMult, int samplesDOF,
            double apertureRadius, double focalLength, Image outputImage)
        {
            // Color to be accumulated with AA and DOF if enabled
            Color pixelColor = new Color(0, 0, 0);

            // Anti-aliasing (multiple samples per pixel)
            for (int sampleX = 0; sampleX < AAMult; sampleX++)
            {
                for (int sampleY = 0; sampleY < AAMult; sampleY++)
                {
                    // Divide pixel into AAMult * AAMult grid
                    double offsetX = (sampleX + 0.5) / AAMult;
                    double offsetY = (sampleY + 0.5) / AAMult;

                    // Multiple samples per AA sample (if DOF enabled)
                    for (int sampleDOF = 0; sampleDOF < samplesDOF; sampleDOF++)
                    {
                        // Fire a ray through this pixel (subpixel if AA enabled)
                        Ray ray = camera.GenerateRay(px, py, offsetX, offsetY, apertureRadius, focalLength);

                        // Trace the ray (see if it hits anything and find its final color)
                        pixelColor += TraceRay(ray); // Accumulate color (for AA and DOF)
                    }
                }
            }

            // Average accumulated pixel
            int totalSamples = AAMult * AAMult * samplesDOF;
            pixelColor /= totalSamples;
            outputImage.SetPixel(px, py, pixelColor);
        }
    }
}
