// ---------------------------------------------------------------------------
// Domain types. These mirror the SceneDto the C# API expects.
//
// The *form* types (MaterialForm, ObjectForm) are intentionally "fat": they
// carry every field any variant could need plus a stable client-side `key`
// for React lists. `toDto` in scene.ts narrows them to the exact wire shape.
// ---------------------------------------------------------------------------

export type Vec3 = [number, number, number];

export type MaterialType = "standard" | "texture" | "procedural";
export type ObjectType = "sphere" | "plane" | "triangle" | "objModel";

export interface RenderSettings {
  width: number;
  height: number;
  aaMultiplier: number;
  ambientLighting: boolean;
}

export interface Camera {
  position: Vec3;
  rotationAxis: Vec3;
  rotationAngle: number;
  scale: number;
}

export interface MaterialForm {
  key: string;
  id: string;
  type: MaterialType;
  // standard
  ambient: Vec3;
  diffuse: Vec3;
  specular: Vec3;
  shininess: number;
  reflectivity: number;
  transmissivity: number;
  refractiveIndex: number;
  // texture
  colorMap: string;
  normalMap: string;
  // procedural
  pattern: string;
  scaleU: number;
  scaleV: number;
  color1: Vec3;
  color2: Vec3;
}

export interface LightForm {
  key: string;
  position: Vec3;
  color: Vec3;
}

export interface ObjectForm {
  key: string;
  type: ObjectType;
  material: string;
  // sphere
  center: Vec3;
  radius: number;
  // plane
  point: Vec3;
  normal: Vec3;
  // triangle
  v0: Vec3;
  v1: Vec3;
  v2: Vec3;
  // objModel
  model: string;
  position: Vec3;
  rotationAxis: Vec3;
  rotationAngle: number;
  scale: number;
}

export interface SceneForm {
  render: RenderSettings;
  camera: Camera;
  materials: MaterialForm[];
  lights: LightForm[];
  objects: ObjectForm[];
}

export interface Limits {
  maxWidth: number;
  maxHeight: number;
  maxAaMultiplier: number;
  maxObjects?: number;
  maxLights?: number;
  maxMaterials?: number;
}

export interface Capabilities {
  materialTypes: MaterialType[];
  patterns: string[];
  objectTypes: ObjectType[];
  textures: string[];
  models: string[];
  limits: Limits;
}

// An entry in /examples/index.json (served from the Next public folder).
export interface ExampleMeta {
  id: string;
  name: string;
  file: string;
  description?: string;
}
