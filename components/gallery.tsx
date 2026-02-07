"use client"

import { useState } from "react"
import Image from "next/image"

const renders = [
  {
    src: "/images/final_scene.png",
    title: "Final Scene",
    description:
      "Textured bunny with mirror spheres, colored walls, depth of field blur, and anti-aliasing at 1000x1000.",
  },
  {
    src: "/images/sample_scene_1_s1.png",
    title: "Basic Primitives",
    description:
      "A simple scene with a plane, sphere, and triangle demonstrating Phong shading and shadows.",
  },
  {
    src: "/images/sample_scene_2_s2.png",
    title: "Cornell Box",
    description:
      "Classic Cornell box with a mirror sphere and glass sphere showing reflections and refractions.",
  },
  {
    src: "/images/AAAfter.png",
    title: "Anti-Aliasing",
    description:
      "Grid-based supersampling eliminates jagged edges for smooth, clean renders.",
  },
  {
    src: "/images/FOBAfter.png",
    title: "Depth of Field",
    description:
      "Physical camera aperture simulation creates natural bokeh and focal plane effects.",
  },
  {
    src: "/images/TEXAfter.png",
    title: "Texture Mapping",
    description:
      "UV-mapped color textures applied to OBJ models using barycentric coordinate interpolation.",
  },
]

export function Gallery() {
  const [selected, setSelected] = useState(0)

  return (
    <section className="px-4 py-20" id="gallery">
      <div className="mx-auto max-w-5xl">
        <h2 className="mb-2 text-center font-mono text-sm tracking-widest uppercase text-primary">
          Gallery
        </h2>
        <p className="mb-12 text-center text-3xl font-bold text-foreground text-balance">
          Renders from the engine
        </p>

        <div className="flex flex-col gap-6 lg:flex-row">
          {/* Main image */}
          <div className="flex-1">
            <div className="relative aspect-square overflow-hidden rounded-lg border border-border bg-card">
              <Image
                src={renders[selected].src}
                alt={renders[selected].title}
                fill
                className="object-cover"
                priority
              />
            </div>
            <div className="mt-4">
              <h3 className="text-lg font-semibold text-foreground">
                {renders[selected].title}
              </h3>
              <p className="mt-1 text-sm leading-relaxed text-muted-foreground">
                {renders[selected].description}
              </p>
            </div>
          </div>

          {/* Thumbnail grid */}
          <div className="grid grid-cols-3 gap-3 lg:w-64 lg:grid-cols-2">
            {renders.map((render, i) => (
              <button
                key={render.src}
                onClick={() => setSelected(i)}
                className={`relative aspect-square overflow-hidden rounded-md border-2 transition-all ${
                  i === selected
                    ? "border-primary ring-2 ring-primary/20"
                    : "border-border hover:border-muted-foreground"
                }`}
              >
                <Image
                  src={render.src}
                  alt={render.title}
                  fill
                  className="object-cover"
                />
                <span className="sr-only">View {render.title}</span>
              </button>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
