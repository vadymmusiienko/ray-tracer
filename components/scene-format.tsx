export function SceneFormat() {
  const exampleScene = `Camera (0, 0, 0) (0, 0, 1) 0 1

Material "Red" (.1, .05, .05) (1, .5, .5) (0, 0, 0) 0 0 0 1
Material "MirrorMat" (0, 0, 0) (0, 0, 0) (0, 0, 0) 0 1 0 1

PointLight "Light" (.75, 1.5, 0) (1, 1, 1)

Plane "Floor" (0, -1, 0) (0, 1, 0) "Red"
Sphere "Sphere" (0, 0, 2) .5 "MirrorMat"`

  return (
    <section className="px-4 py-20" id="features">
      <div className="mx-auto max-w-5xl">
        <h2 className="mb-2 text-center font-mono text-sm tracking-widest uppercase text-primary">
          Scene File Format
        </h2>
        <p className="mb-12 text-center text-3xl font-bold text-foreground text-balance">
          A simple text-based scene definition
        </p>

        <div className="flex flex-col gap-8 lg:flex-row">
          <div className="flex-1 rounded-lg border border-border bg-card p-6">
            <pre className="overflow-x-auto font-mono text-sm leading-relaxed text-secondary-foreground">
              <code>{exampleScene}</code>
            </pre>
          </div>

          <div className="flex-1 space-y-4">
            <div className="rounded-lg border border-border bg-card p-4">
              <h4 className="font-mono text-xs uppercase tracking-wide text-primary">
                Camera
              </h4>
              <p className="mt-1 text-sm text-muted-foreground">
                Position, rotation axis, angle, and scale. Defines the
                viewpoint for the render.
              </p>
            </div>
            <div className="rounded-lg border border-border bg-card p-4">
              <h4 className="font-mono text-xs uppercase tracking-wide text-primary">
                Material
              </h4>
              <p className="mt-1 text-sm text-muted-foreground">
                Ambient, diffuse, specular colors, shininess, reflectivity,
                transmissivity, and refractive index.
              </p>
            </div>
            <div className="rounded-lg border border-border bg-card p-4">
              <h4 className="font-mono text-xs uppercase tracking-wide text-primary">
                Primitives
              </h4>
              <p className="mt-1 text-sm text-muted-foreground">
                Spheres, planes, and triangles with position, dimensions, and
                material references.
              </p>
            </div>
            <div className="rounded-lg border border-border bg-card p-4">
              <h4 className="font-mono text-xs uppercase tracking-wide text-primary">
                Lights
              </h4>
              <p className="mt-1 text-sm text-muted-foreground">
                Point lights with position and color. Multiple lights supported
                for complex illumination.
              </p>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
