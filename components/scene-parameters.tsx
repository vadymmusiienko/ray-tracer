"use client"

import { useState, useEffect } from "react"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Slider } from "@/components/ui/slider"

const MIN_DIM = 100
const MAX_DIM = 4000
const DIM_STEP = 100

function clampDim(value: number): number {
  const rounded = Math.round(value / DIM_STEP) * DIM_STEP
  return Math.max(MIN_DIM, Math.min(MAX_DIM, rounded))
}

export interface SceneParams {
  width: number
  height: number
  aaMultiplier: number
  ambientLighting: boolean
  apertureRadius: number
  focalLength: number
}

interface SceneParametersProps {
  params: SceneParams
  onChange: (params: SceneParams) => void
}

function DimensionInput({
  id,
  label,
  value,
  onCommit,
}: {
  id: string
  label: string
  value: number
  onCommit: (v: number) => void
}) {
  const [draft, setDraft] = useState(String(value))

  useEffect(() => {
    setDraft(String(value))
  }, [value])

  function commit() {
    const parsed = parseInt(draft)
    if (isNaN(parsed) || parsed < MIN_DIM) {
      const clamped = clampDim(isNaN(parsed) ? MIN_DIM : parsed)
      setDraft(String(clamped))
      onCommit(clamped)
    } else {
      const clamped = clampDim(parsed)
      setDraft(String(clamped))
      onCommit(clamped)
    }
  }

  return (
    <div className="flex-1 space-y-1.5">
      <Label htmlFor={id} className="text-xs text-muted-foreground">
        {label}
      </Label>
      <Input
        id={id}
        type="text"
        inputMode="numeric"
        value={draft}
        onChange={(e) => setDraft(e.target.value.replace(/[^0-9]/g, ""))}
        onBlur={commit}
        onKeyDown={(e) => {
          if (e.key === "Enter") commit()
        }}
        className="font-mono text-sm"
      />
      <p className="text-xs text-muted-foreground">
        {MIN_DIM} - {MAX_DIM}, step {DIM_STEP}
      </p>
    </div>
  )
}

export function SceneParameters({ params, onChange }: SceneParametersProps) {
  return (
    <div className="space-y-5">
      <div>
        <h3 className="mb-3 font-mono text-xs uppercase tracking-widest text-primary">
          Render Settings
        </h3>
        <p className="mb-4 text-xs leading-relaxed text-muted-foreground">
          Configure the command-line arguments for the ray tracer. These
          correspond to the flags passed when running the engine.
        </p>
      </div>

      <div className="flex gap-3">
        <DimensionInput
          id="width"
          label="Width (px)"
          value={params.width}
          onCommit={(w) => onChange({ ...params, width: w })}
        />
        <DimensionInput
          id="height"
          label="Height (px)"
          value={params.height}
          onCommit={(h) => onChange({ ...params, height: h })}
        />
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="aa" className="text-xs text-muted-foreground">
            Anti-Aliasing ({params.aaMultiplier}x = {params.aaMultiplier * params.aaMultiplier} samples/px)
          </Label>
        </div>
        <Slider
          id="aa"
          min={1}
          max={4}
          step={1}
          value={[params.aaMultiplier]}
          onValueChange={([val]) =>
            onChange({ ...params, aaMultiplier: val })
          }
        />
        <p className="text-xs text-muted-foreground">
          Flag: <code className="font-mono text-primary">-x {params.aaMultiplier}</code>
        </p>
      </div>

      <div className="flex items-center justify-between rounded-md border border-border p-3">
        <div>
          <Label htmlFor="ambient" className="text-sm">
            Ambient Lighting
          </Label>
          <p className="text-xs text-muted-foreground">
            Flag: <code className="font-mono text-primary">-l</code>
          </p>
        </div>
        <Switch
          id="ambient"
          checked={params.ambientLighting}
          onCheckedChange={(checked) =>
            onChange({ ...params, ambientLighting: checked })
          }
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="aperture" className="text-xs text-muted-foreground">
          Aperture Radius ({params.apertureRadius.toFixed(2)})
        </Label>
        <Slider
          id="aperture"
          min={0}
          max={0.5}
          step={0.01}
          value={[params.apertureRadius]}
          onValueChange={([val]) =>
            onChange({ ...params, apertureRadius: val })
          }
        />
        <p className="text-xs text-muted-foreground">
          Flag: <code className="font-mono text-primary">-r {params.apertureRadius.toFixed(2)}</code>
          {" "}(0 = no depth of field)
        </p>
      </div>

      <div className="space-y-2">
        <Label htmlFor="focal" className="text-xs text-muted-foreground">
          Focal Length ({params.focalLength.toFixed(1)})
        </Label>
        <Slider
          id="focal"
          min={0.5}
          max={10}
          step={0.1}
          value={[params.focalLength]}
          onValueChange={([val]) =>
            onChange({ ...params, focalLength: val })
          }
        />
        <p className="text-xs text-muted-foreground">
          Flag: <code className="font-mono text-primary">-t {params.focalLength.toFixed(1)}</code>
        </p>
      </div>

      <div className="rounded-md border border-border bg-secondary/50 p-3">
        <p className="font-mono text-xs text-muted-foreground">
          <span className="text-foreground">$</span> dotnet run -- -f scene.txt -w {params.width} -h {params.height} -x {params.aaMultiplier}{params.ambientLighting ? " -l" : ""}{params.apertureRadius > 0 ? ` -r ${params.apertureRadius.toFixed(2)} -t ${params.focalLength.toFixed(1)}` : ""}
        </p>
      </div>
    </div>
  )
}
