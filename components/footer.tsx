import Link from "next/link"

export function Footer() {
  return (
    <footer className="border-t border-border bg-card">
      <div className="mx-auto flex max-w-5xl flex-col items-center justify-between gap-4 px-4 py-8 md:flex-row">
        <p className="text-sm text-muted-foreground">
          Built by Vadym Musiienko. A university ray tracing project in C#.
        </p>
        <div className="flex items-center gap-6">
          <a
            href="https://github.com/vadymmusiienko/ray-tracer"
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            GitHub
          </a>
          <Link
            href="/generate"
            className="text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            Scene Generator
          </Link>
        </div>
      </div>
    </footer>
  )
}
