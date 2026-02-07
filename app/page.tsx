import { Navbar } from "@/components/navbar"
import { Hero } from "@/components/hero"
import { Gallery } from "@/components/gallery"
import { Features } from "@/components/features"
import { SceneFormat } from "@/components/scene-format"
import { Footer } from "@/components/footer"

export default function Home() {
  return (
    <main className="min-h-screen">
      <Navbar />
      <Hero />
      <Gallery />
      <Features />
      <SceneFormat />
      <Footer />
    </main>
  )
}
