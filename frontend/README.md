# Ray Tracer — Scene Builder (Next.js)

A Next.js (App Router + TypeScript) frontend for the C# ray tracer. You build a
scene with a form, hit **Render**, and the C# API returns a PNG.

This is intended to drop into a larger Next.js portfolio later — it's a normal
App Router project with components under `src/`, a typed API client, and no
exotic dependencies (CSS Modules, no UI framework).

## Architecture

```
Browser ──/api/*──► Next server ──rewrite──► C# render API (:5099)
        ──/examples/*──► Next public folder
```

The browser only ever talks to the Next origin. `next.config.ts` rewrites
`/api/*` to the C# backend, so there's no CORS to configure. Example scenes are
static JSON in `public/examples/`.

## Running

1. Start the C# API (from the repo root):

   ```bash
   DOTNET_ROLL_FORWARD=Major dotnet run --project src/RayTracer.Web
   # listens on http://localhost:5099
   ```

2. Start the frontend:

   ```bash
   cd frontend
   npm install
   npm run dev          # http://localhost:3000
   ```

If your API runs elsewhere, set `RAYTRACER_API_ORIGIN` (see `.env.example`).

## Layout

- `src/lib/types.ts` — scene/form/capabilities types (mirror the API's `SceneDto`).
- `src/lib/scene.ts` — `toDto` / `fromDto` between form state and the wire format.
- `src/lib/api.ts` — typed client for `/api/render`, `/api/capabilities`, examples.
- `src/lib/defaults.ts` — factory defaults for materials/lights/objects.
- `src/components/SceneBuilder.tsx` — owns scene state and orchestrates panels.
- `src/components/panels/*` — render settings, camera, materials, lights, objects.
- `src/components/fields/*` — reusable controlled inputs (number, vec3, color, …).
- `src/components/Preview.tsx` — status, validation errors, rendered image.

## Scripts

- `npm run dev` — dev server
- `npm run build` / `npm start` — production build & serve
- `npm run typecheck` — `tsc --noEmit`
- `npm run lint` — ESLint
