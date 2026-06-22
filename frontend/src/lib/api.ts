import type { Capabilities, ExampleMeta } from "./types";

// All requests go to the Next origin; /api/* is proxied to the C# backend by
// next.config.ts rewrites, and /examples/* is served from the public folder.

// A render failure carrying the validation messages the API returned (if any).
export class RenderError extends Error {
  readonly messages: string[];
  constructor(messages: string[]) {
    super(messages[0] ?? "Render failed");
    this.name = "RenderError";
    this.messages = messages;
  }
}

export async function fetchCapabilities(): Promise<Capabilities> {
  const res = await fetch("/api/capabilities");
  if (!res.ok)
    throw new Error(`Capabilities request failed (HTTP ${res.status})`);
  return (await res.json()) as Capabilities;
}

// POST a SceneDto and return the rendered PNG as a Blob, or throw RenderError.
export async function renderScene(
  dto: unknown,
  signal?: AbortSignal,
): Promise<Blob> {
  const res = await fetch("/api/render", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
    signal,
  });

  if (res.ok) return res.blob();

  const data = await res
    .json()
    .catch(() => ({ errors: [`HTTP ${res.status}`] }));
  const messages: string[] = data.errors ?? [
    data.error ?? `HTTP ${res.status}`,
  ];
  throw new RenderError(messages);
}

export async function fetchExampleIndex(): Promise<ExampleMeta[]> {
  const res = await fetch("/examples/index.json");
  if (!res.ok) throw new Error(`Examples index failed (HTTP ${res.status})`);
  return (await res.json()) as ExampleMeta[];
}

export async function fetchExample(file: string): Promise<unknown> {
  const res = await fetch(`/examples/${file}`);
  if (!res.ok) throw new Error(`Example load failed (HTTP ${res.status})`);
  return res.json();
}
