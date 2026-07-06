"use client"

import { useState, useRef, useEffect } from "react"
import { useChat } from "@ai-sdk/react"
import { DefaultChatTransport } from "ai"
import { Send, Sparkles, Loader2, ArrowLeft } from "lucide-react"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import {
  SceneParameters,
  type SceneParams,
} from "@/components/scene-parameters"
import { SceneOutput } from "@/components/scene-output"

function getUIMessageText(
  msg: { parts?: Array<{ type: string; text?: string }> }
): string {
  if (!msg.parts || !Array.isArray(msg.parts)) return ""
  return msg.parts
    .filter(
      (p): p is { type: "text"; text: string } => p.type === "text"
    )
    .map((p) => p.text)
    .join("")
}

function extractSceneFile(text: string): string | null {
  // Try to extract scene file from ```scene blocks first
  const sceneMatch = text.match(/```scene\n([\s\S]*?)```/)
  if (sceneMatch) return sceneMatch[1].trim()

  // Fallback to generic code blocks that look like scene files
  const codeMatch = text.match(/```(?:\w*)\n([\s\S]*?)```/)
  if (codeMatch) {
    const content = codeMatch[1].trim()
    if (content.startsWith("Camera")) return content
  }

  return null
}

const SUGGESTIONS = [
  "A Cornell box with a mirror sphere and a glass sphere",
  "A simple scene with three colorful spheres on a grey floor",
  "A checkerboard floor with a reflective sphere under dramatic lighting",
  "An abstract arrangement of geometric shapes with multiple colored lights",
]

export default function GeneratePage() {
  const [input, setInput] = useState("")
  const [params, setParams] = useState<SceneParams>({
    width: 400,
    height: 400,
    aaMultiplier: 1,
    ambientLighting: false,
    apertureRadius: 0,
    focalLength: 1,
  })

  const messagesEndRef = useRef<HTMLDivElement>(null)
  const textareaRef = useRef<HTMLTextAreaElement>(null)

  const { messages, sendMessage, status } = useChat({
    transport: new DefaultChatTransport({ api: "/api/chat" }),
  })

  const isLoading = status === "streaming" || status === "submitted"

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }, [messages])

  function handleSubmit(text?: string) {
    const messageText = text || input.trim()
    if (!messageText || isLoading) return
    sendMessage({ text: messageText })
    setInput("")
  }

  // Find the latest scene file from messages
  let latestScene: string | null = null
  for (let i = messages.length - 1; i >= 0; i--) {
    const text = getUIMessageText(messages[i])
    const scene = extractSceneFile(text)
    if (scene) {
      latestScene = scene
      break
    }
  }

  const command = `dotnet run -- -f scene.txt -w ${params.width} -h ${params.height} -x ${params.aaMultiplier}${params.ambientLighting ? " -l" : ""}${params.apertureRadius > 0 ? ` -r ${params.apertureRadius.toFixed(2)} -t ${params.focalLength.toFixed(1)}` : ""}`

  return (
    <div className="flex min-h-screen flex-col">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-border bg-background/80 backdrop-blur-md">
        <div className="mx-auto flex h-14 max-w-7xl items-center gap-4 px-4">
          <Button asChild variant="ghost" size="sm" className="gap-1.5">
            <Link href="/">
              <ArrowLeft className="h-4 w-4" />
              Back
            </Link>
          </Button>
          <div className="h-5 w-px bg-border" />
          <h1 className="font-mono text-sm font-semibold text-foreground">
            Scene Generator
          </h1>
        </div>
      </header>

      {/* Main content */}
      <div className="flex flex-1 flex-col lg:flex-row">
        {/* Left sidebar - Parameters */}
        <aside className="w-full border-b border-border bg-card p-5 lg:w-80 lg:border-b-0 lg:border-r lg:overflow-y-auto">
          <SceneParameters params={params} onChange={setParams} />
        </aside>

        {/* Center - Chat */}
        <div className="flex flex-1 flex-col">
          <div className="flex-1 overflow-y-auto p-4">
            {messages.length === 0 ? (
              <div className="flex h-full flex-col items-center justify-center">
                <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
                  <Sparkles className="h-6 w-6 text-primary" />
                </div>
                <h2 className="mt-4 text-lg font-semibold text-foreground">
                  Describe your scene
                </h2>
                <p className="mt-2 max-w-md text-center text-sm leading-relaxed text-muted-foreground">
                  Tell the AI what you want to render and it will generate a
                  scene file compatible with the ray tracer. Try one of the
                  suggestions below.
                </p>
                <div className="mt-6 grid grid-cols-1 gap-2 sm:grid-cols-2">
                  {SUGGESTIONS.map((suggestion) => (
                    <button
                      key={suggestion}
                      onClick={() => handleSubmit(suggestion)}
                      className="rounded-lg border border-border bg-card px-4 py-3 text-left text-sm text-muted-foreground transition-colors hover:border-primary/30 hover:text-foreground"
                    >
                      {suggestion}
                    </button>
                  ))}
                </div>
              </div>
            ) : (
              <div className="mx-auto max-w-2xl space-y-6">
                {messages.map((message) => {
                  const text = getUIMessageText(message)
                  if (!text) return null

                  const isUser = message.role === "user"

                  // Split text to render scene blocks specially
                  const scene = extractSceneFile(text)
                  const textWithoutCode = text
                    .replace(/```(?:scene|\w*)\n[\s\S]*?```/g, "")
                    .trim()

                  return (
                    <div
                      key={message.id}
                      className={`flex ${isUser ? "justify-end" : "justify-start"}`}
                    >
                      <div
                        className={`max-w-xl rounded-lg px-4 py-3 ${
                          isUser
                            ? "bg-primary text-primary-foreground"
                            : "border border-border bg-card text-card-foreground"
                        }`}
                      >
                        {textWithoutCode && (
                          <p className="text-sm leading-relaxed whitespace-pre-wrap">
                            {textWithoutCode}
                          </p>
                        )}
                        {scene && !isUser && (
                          <div className="mt-3 rounded-md border border-border bg-secondary/50 p-3">
                            <pre className="max-h-48 overflow-auto font-mono text-xs leading-relaxed text-foreground">
                              {scene}
                            </pre>
                          </div>
                        )}
                      </div>
                    </div>
                  )
                })}

                {isLoading && messages[messages.length - 1]?.role === "user" && (
                  <div className="flex justify-start">
                    <div className="flex items-center gap-2 rounded-lg border border-border bg-card px-4 py-3">
                      <Loader2 className="h-4 w-4 animate-spin text-primary" />
                      <span className="text-sm text-muted-foreground">
                        Generating scene...
                      </span>
                    </div>
                  </div>
                )}

                <div ref={messagesEndRef} />
              </div>
            )}
          </div>

          {/* Input */}
          <div className="border-t border-border bg-card p-4">
            <form
              onSubmit={(e) => {
                e.preventDefault()
                handleSubmit()
              }}
              className="mx-auto flex max-w-2xl items-end gap-2"
            >
              <Textarea
                ref={textareaRef}
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !e.shiftKey) {
                    e.preventDefault()
                    handleSubmit()
                  }
                }}
                placeholder="Describe the scene you want to render..."
                className="min-h-[44px] max-h-32 resize-none font-sans text-sm"
                rows={1}
                disabled={isLoading}
              />
              <Button
                type="submit"
                size="icon"
                disabled={!input.trim() || isLoading}
                className="h-11 w-11 shrink-0"
              >
                <Send className="h-4 w-4" />
                <span className="sr-only">Send message</span>
              </Button>
            </form>
          </div>
        </div>

        {/* Right sidebar - Output */}
        {latestScene && (
          <aside className="w-full border-t border-border bg-card p-5 lg:w-96 lg:border-l lg:border-t-0 lg:overflow-y-auto">
            <SceneOutput content={latestScene} command={command} />
          </aside>
        )}
      </div>
    </div>
  )
}
