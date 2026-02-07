"use client"

import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Slider } from "@/components/ui/slider"

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
        <div className="flex-1 space-y-1.5">
          <Label htmlFor="width" className="text-xs text-muted-foreground">
            Width (px)
          </Label>
          <Input
            id="width"
            type="number"
            min={100}
            max={2000}
            value={params.width}
            onChange={(e) =>
              onChange({ ...params, width: parseInt(e.target.value) || 400 })
            }
            className="font-mono text-sm"
          />
        </div>
        <div className="flex-1 space-y-1.5">
          <Label htmlFor="height" className="text-xs text-muted-foreground">
            Height (px)
          </Label>
          <Input
            id="height"
            type="number"
            min={100}
            max={2000}
            value={params.height}
            onChange={(e) =>
              onChange({ ...params, height: parseInt(e.target.value) || 400 })
            }
            className="font-mono text-sm"
          />
        </div>
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
