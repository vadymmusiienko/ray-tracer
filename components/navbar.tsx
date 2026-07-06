import Link from "next/link"
import { Button } from "@/components/ui/button"

export function Navbar() {
  return (
    <header className="fixed top-0 z-50 w-full border-b border-border/50 bg-background/80 backdrop-blur-md">
      <nav className="mx-auto flex h-16 max-w-5xl items-center justify-between px-4">
        <Link href="/" className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-md bg-primary">
            <svg
              viewBox="0 0 24 24"
              fill="none"
              className="h-5 w-5 text-primary-foreground"
              aria-hidden="true"
            >
              <path
                d="M12 2L2 19h20L12 2z"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinejoin="round"
              />
              <circle cx="12" cy="13" r="3" fill="currentColor" />
            </svg>
          </div>
          <span className="font-mono text-sm font-semibold tracking-tight text-foreground">
            RayTracer
          </span>
        </Link>

        <div className="flex items-center gap-6">
          <Link
            href="/#gallery"
            className="hidden text-sm text-muted-foreground transition-colors hover:text-foreground md:block"
          >
            Gallery
          </Link>
          <Link
            href="/#features"
            className="hidden text-sm text-muted-foreground transition-colors hover:text-foreground md:block"
          >
            Features
          </Link>
          <Button asChild size="sm" className="font-mono">
            <Link href="/generate">Generate</Link>
          </Button>
        </div>
      </nav>
    </header>
  )
}
