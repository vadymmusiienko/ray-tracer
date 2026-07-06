"use client"

import { Copy, Download, Check } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useState } from "react"

interface SceneOutputProps {
  content: string
  command: string
}

export function SceneOutput({ content, command }: SceneOutputProps) {
  const [copied, setCopied] = useState<"scene" | "command" | null>(null)

  function handleCopy(text: string, type: "scene" | "command") {
    navigator.clipboard.writeText(text)
    setCopied(type)
    setTimeout(() => setCopied(null), 2000)
  }

  function handleDownload() {
    const blob = new Blob([content], { type: "text/plain" })
    const url = URL.createObjectURL(blob)
    const a = document.createElement("a")
    a.href = url
    a.download = "scene.txt"
    a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="font-mono text-xs uppercase tracking-widest text-primary">
          Generated Scene File
        </h3>
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleCopy(content, "scene")}
            className="h-7 gap-1.5 px-2 text-xs"
          >
            {copied === "scene" ? (
              <Check className="h-3 w-3" />
            ) : (
              <Copy className="h-3 w-3" />
            )}
            {copied === "scene" ? "Copied" : "Copy"}
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={handleDownload}
            className="h-7 gap-1.5 px-2 text-xs"
          >
            <Download className="h-3 w-3" />
            Download
          </Button>
        </div>
      </div>

      <div className="max-h-72 overflow-auto rounded-md border border-border bg-secondary/50 p-4">
        <pre className="font-mono text-xs leading-relaxed text-foreground whitespace-pre-wrap">
          {content}
        </pre>
      </div>

      <div className="space-y-1.5">
        <p className="font-mono text-xs uppercase tracking-widest text-primary">
          Run Command
        </p>
        <div className="flex items-center gap-2">
          <div className="flex-1 overflow-x-auto rounded-md border border-border bg-secondary/50 px-3 py-2">
            <code className="font-mono text-xs text-foreground whitespace-nowrap">
              {command}
            </code>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleCopy(command, "command")}
            className="h-8 w-8 shrink-0 p-0"
          >
            {copied === "command" ? (
              <Check className="h-3 w-3" />
            ) : (
              <Copy className="h-3 w-3" />
            )}
            <span className="sr-only">Copy command</span>
          </Button>
        </div>
      </div>
    </div>
  )
}
