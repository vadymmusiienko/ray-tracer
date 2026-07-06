import Link from "next/link"
import { ArrowRight } from "lucide-react"
import { Button } from "@/components/ui/button"

export function Hero() {
  return (
    <section className="flex flex-col items-center justify-center px-4 pt-32 pb-20 text-center">
      <p className="mb-4 font-mono text-sm tracking-widest uppercase text-primary">
        C# Ray Tracing Engine
      </p>
      <h1 className="max-w-3xl text-5xl font-bold leading-tight tracking-tight text-balance md:text-7xl text-foreground">
        Photorealistic rendering, one ray at a time
      </h1>
      <p className="mt-6 max-w-xl text-lg leading-relaxed text-muted-foreground text-pretty">
        A full-featured ray tracer built from scratch in C# supporting
        reflections, refractions, anti-aliasing, depth of field, OBJ models,
        and texture mapping. Powered by BVH acceleration.
      </p>
      <div className="mt-10 flex items-center gap-4">
        <Button asChild size="lg" className="font-mono">
          <Link href="/generate">
            Generate a Scene
            <ArrowRight className="ml-2 h-4 w-4" />
          </Link>
        </Button>
        <Button asChild variant="outline" size="lg" className="font-mono">
          <a
            href="https://github.com/vadymmusiienko/ray-tracer"
            target="_blank"
            rel="noopener noreferrer"
          >
            View on GitHub
          </a>
        </Button>
      </div>
    </section>
  )
}
