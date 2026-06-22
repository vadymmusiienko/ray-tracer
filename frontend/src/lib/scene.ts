import { emptyScene, newLight, newMaterial, newObject } from "./defaults";
import type { Capabilities, SceneForm } from "./types";

// ---------------------------------------------------------------------------
// Form state <-> API wire format (SceneDto).
// ---------------------------------------------------------------------------

// Narrow the fat form state down to the exact JSON the API accepts, picking
// only the fields relevant to each material/object variant.
export function toDto(scene: SceneForm): unknown {
  const materials = scene.materials.map((m) => {
    if (m.type === "standard") {
      return {
        id: m.id,
        type: "standard",
        ambient: m.ambient,
        diffuse: m.diffuse,
        specular: m.specular,
        shininess: m.shininess,
        reflectivity: m.reflectivity,
        transmissivity: m.transmissivity,
        refractiveIndex: m.refractiveIndex,
      };
    }
    if (m.type === "texture") {
      return {
        id: m.id,
        type: "texture",
        colorMap: m.colorMap,
        ...(m.normalMap ? { normalMap: m.normalMap } : {}),
      };
    }
    return {
      id: m.id,
      type: "procedural",
      pattern: m.pattern,
      scaleU: m.scaleU,
      scaleV: m.scaleV,
      color1: m.color1,
      color2: m.color2,
    };
  });

  const objects = scene.objects.map((o) => {
    const base = { type: o.type, material: o.material };
    if (o.type === "sphere")
      return { ...base, center: o.center, radius: o.radius };
    if (o.type === "plane")
      return { ...base, point: o.point, normal: o.normal };
    if (o.type === "triangle") return { ...base, v0: o.v0, v1: o.v1, v2: o.v2 };
    return {
      ...base,
      model: o.model,
      position: o.position,
      rotationAxis: o.rotationAxis,
      rotationAngle: o.rotationAngle,
      scale: o.scale,
    };
  });

  return {
    render: { ...scene.render },
    camera: { ...scene.camera },
    materials,
    lights: scene.lights.map((l) => ({ position: l.position, color: l.color })),
    objects,
  };
}

// Layer a loaded SceneDto over the factory defaults so every field the form
// touches is present, regardless of which fields the example actually set.
// `caps` seeds sensible defaults (e.g. first available texture/model).
export function fromDto(dto: unknown, caps: Capabilities): SceneForm {
  const d = (dto ?? {}) as Record<string, unknown>;
  const render = (d.render ?? {}) as Record<string, unknown>;
  const base = emptyScene();

  return {
    render: {
      width: num(render.width, 400),
      height: num(render.height, 400),
      aaMultiplier: num(render.aaMultiplier, 1),
      ambientLighting: Boolean(render.ambientLighting ?? false),
    },
    camera: { ...base.camera, ...((d.camera ?? {}) as object) },
    materials: asArray(d.materials).map((m) => ({
      ...newMaterial(caps),
      ...(m as object),
    })),
    lights: asArray(d.lights).map((l) => ({
      ...newLight(),
      ...(l as object),
    })),
    objects: asArray(d.objects).map((o) => ({
      ...newObject(undefined, caps),
      ...(o as object),
    })),
  };
}

function num(value: unknown, fallback: number): number {
  return typeof value === "number" && Number.isFinite(value) ? value : fallback;
}

function asArray(value: unknown): unknown[] {
  return Array.isArray(value) ? value : [];
}
