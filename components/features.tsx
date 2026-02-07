import {
  Layers,
  Focus,
  Sparkles,
  Box,
  RefreshCw,
  Paintbrush,
} from "lucide-react"

const features = [
  {
    icon: Sparkles,
    title: "Anti-Aliasing (SSAA)",
    description:
      "Grid-based supersampling with configurable multiplier for smooth, artifact-free edges.",
  },
  {
    icon: Focus,
    title: "Depth of Field",
    description:
      "Physical camera simulation with aperture radius and focal length for realistic blur effects.",
  },
  {
    icon: RefreshCw,
    title: "Reflections & Refractions",
    description:
      "Recursive ray tracing with configurable reflectivity, transmissivity, and refractive index.",
  },
  {
    icon: Paintbrush,
    title: "Texture Mapping",
    description:
      "UV-mapped color textures and procedural patterns using barycentric coordinate interpolation.",
  },
  {
    icon: Box,
    title: "OBJ Model Support",
    description:
      "Load complex 3D models with BVH tree acceleration for orders-of-magnitude faster renders.",
  },
  {
    icon: Layers,
    title: "Phong Shading",
    description:
      "Full Phong illumination model with ambient, diffuse, and specular components plus shadow rays.",
  },
]

export function Features() {
  return (
    <section className="px-4 py-20">
      <div className="mx-auto max-w-5xl">
        <h2 className="mb-2 text-center font-mono text-sm tracking-widest uppercase text-primary">
          Features
        </h2>
        <p className="mb-12 text-center text-3xl font-bold text-foreground text-balance">
          Everything you need for photorealistic rendering
        </p>
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {features.map((feature) => (
            <div
              key={feature.title}
              className="rounded-lg border border-border bg-card p-6 transition-colors hover:border-primary/30"
            >
              <feature.icon className="mb-4 h-8 w-8 text-primary" />
              <h3 className="mb-2 text-lg font-semibold text-card-foreground">
                {feature.title}
              </h3>
              <p className="text-sm leading-relaxed text-muted-foreground">
                {feature.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
