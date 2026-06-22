import type {
  Capabilities,
  LightForm,
  MaterialForm,
  ObjectForm,
  SceneForm,
} from "./types";

// Stable client-side keys for React lists (user-editable `id`s are not stable).
export function makeKey(): string {
  return Math.random().toString(36).slice(2, 10);
}

let materialSeq = 0;

export function newMaterial(caps?: Capabilities): MaterialForm {
  return {
    key: makeKey(),
    id: `material${++materialSeq}`,
    type: "standard",
    ambient: [0.1, 0.1, 0.1],
    diffuse: [0.8, 0.8, 0.8],
    specular: [1, 1, 1],
    shininess: 30,
    reflectivity: 0,
    transmissivity: 0,
    refractiveIndex: 1,
    colorMap: caps?.textures[0] ?? "",
    normalMap: "",
    pattern: "Checkers",
    scaleU: 10,
    scaleV: 10,
    color1: [1, 0, 0],
    color2: [1, 1, 1],
  };
}

export function newLight(): LightForm {
  return { key: makeKey(), position: [0, 1, 0], color: [1, 1, 1] };
}

export function newObject(scene?: SceneForm, caps?: Capabilities): ObjectForm {
  return {
    key: makeKey(),
    type: "sphere",
    material: scene?.materials[0]?.id ?? "",
    center: [0, 0, 2],
    radius: 0.5,
    point: [0, -1, 0],
    normal: [0, 1, 0],
    v0: [-0.5, -0.5, 2],
    v1: [0.5, -0.5, 2],
    v2: [0, 0.5, 2],
    model: caps?.models[0] ?? "",
    position: [0, -0.9, 2],
    rotationAxis: [0, 0, 1],
    rotationAngle: 0,
    scale: 1,
  };
}

export function emptyScene(): SceneForm {
  return {
    render: {
      width: 400,
      height: 400,
      aaMultiplier: 1,
      ambientLighting: false,
    },
    camera: {
      position: [0, 0, 0],
      rotationAxis: [0, 0, 1],
      rotationAngle: 0,
      scale: 1,
    },
    materials: [],
    lights: [],
    objects: [],
  };
}

export const FALLBACK_CAPABILITIES: Capabilities = {
  materialTypes: ["standard", "texture", "procedural"],
  patterns: ["Checkers", "Stripes"],
  objectTypes: ["sphere", "plane", "triangle", "objModel"],
  textures: [],
  models: [],
  limits: { maxWidth: 600, maxHeight: 600, maxAaMultiplier: 2 },
};
